// <copyright>
// Copyright 2022-2023 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Internal;

#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented

namespace ServiceModel.Grpc.Client.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
public readonly ref struct ServerStreamingCall<TRequest, TResponseHeader, TResponse, TResponseValue>
    where TRequest : class
    where TResponse : class, IMessage<TResponseValue>
    where TResponseHeader : class
{
    //// ReSharper disable StaticMemberInGenericType
    private static readonly Action<IClientFilterContext> BlockingFilterLast = FilterLast;
    private static readonly Func<IClientFilterContext, ValueTask> AsyncFilterLast = FilterLastAsync;
    private static readonly Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, IAsyncEnumerable<TResponseValue?>> GetStream = (_, stream) => stream;
    //// ReSharper restore StaticMemberInGenericType

    private readonly GrpcMethod<Message, TRequest, TResponseHeader, TResponse> _method;
    private readonly CallInvoker _callInvoker;
    private readonly IClientCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly CallContext? _callContext;
    private readonly CallOptions _callOptions;

    public ServerStreamingCall(
        IMethod method,
        CallInvoker callInvoker,
        in CallOptionsBuilder callOptionsBuilder,
        IClientCallFilterHandlerFactory? filterHandlerFactory)
    {
        _method = (GrpcMethod<Message, TRequest, TResponseHeader, TResponse>)method;
        _callInvoker = callInvoker;
        _filterHandlerFactory = filterHandlerFactory;

        _callContext = callOptionsBuilder.CallContext;
        _callOptions = callOptionsBuilder.Build();
    }

    public IAsyncEnumerable<TResponseValue?> Invoke(TRequest request)
    {
        var filter = CreateFilter(request);

        IAsyncEnumerable<TResponseValue?> result;
        if (filter == null)
        {
            var call = _callInvoker.AsyncServerStreamingCall(_method, null, _callOptions, request);
            result = ReadServerStreamAsync(call, _callContext, _callOptions.CancellationToken);
        }
        else
        {
            filter.Invoke(BlockingFilterLast);
            var stream = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw().Stream;
            result = (IAsyncEnumerable<TResponseValue?>)stream!;
        }

        return result;
    }

    public Task<IAsyncEnumerable<TResponseValue?>> InvokeAsync(TRequest request) => InvokeAsync(request, GetStream);

    public Task<TResult> InvokeAsync<TResult>(
        TRequest request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
    {
        var filter = CreateFilter(request);
        if (filter == null)
        {
            var call = _callInvoker.AsyncServerStreamingCall(_method, null, _callOptions, request);
            return InvokeCoreAsync(call, _method.ResponseHeaderMarshaller, _callContext, _callOptions, continuationFunction);
        }

        return InvokeWithFilterAsync(filter, continuationFunction);
    }

    private static async Task<TResult> InvokeCoreAsync<TResult>(
        AsyncServerStreamingCall<TResponse> call,
        Marshaller<TResponseHeader>? responseHeaderMarshaller,
        CallContext? callContext,
        CallOptions callOptions,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
    {
        var header = await ReadResponseHeaderAsync(call, responseHeaderMarshaller, callContext, callOptions.CancellationToken).ConfigureAwait(false);
        var stream = ReadServerStreamAsync(call, callContext, callOptions.CancellationToken);

        return continuationFunction(header!, stream);
    }

    private static async Task<TResult> InvokeWithFilterAsync<TResult>(
        IClientCallFilterHandler filter,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
    {
        await filter.InvokeAsync(AsyncFilterLast).ConfigureAwait(false);

        var (responseHeader, response) = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw();
        var stream = (IAsyncEnumerable<TResponseValue?>)response!;
        var header = (TResponseHeader?)responseHeader;

        return continuationFunction(header!, stream);
    }

    private static async Task<TResponseHeader?> ReadResponseHeaderAsync(
        AsyncServerStreamingCall<TResponse> call,
        Marshaller<TResponseHeader>? marshaller,
        CallContext? context,
        CancellationToken token)
    {
        TResponseHeader? result = default;
        try
        {
            Metadata? headers = default;
            if (context != null || marshaller != null)
            {
                headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }
            }

            if (marshaller != null)
            {
                // see ExceptionHandlingTest.ThrowApplicationExceptionServerStreamingHeader
                // gRPC core channel: headers.Count == 0, exception comes on MoveNext
                // gRPC .net channel: headers contains exception details, provided by server error handler
                if (CompatibilityTools.ContainsMethodOutputHeader(headers))
                {
                    result = CompatibilityTools.DeserializeMethodOutputHeader(marshaller, headers);
                }
                else
                {
                    await ClientChannelAdapter.WaitForServerStreamExceptionAsync(call.ResponseStream, headers, marshaller, token).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            call.Dispose();
            throw;
        }

        return result;
    }

    private static async IAsyncEnumerable<TResponseValue?> ReadServerStreamAsync(
        AsyncServerStreamingCall<TResponse> call,
        CallContext? context,
        [EnumeratorCancellation] CancellationToken token)
    {
        using (call)
        {
            if (context != null && !context.ServerResponse.HasValue && !token.IsCancellationRequested)
            {
                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                context.ServerResponse = new ServerResponse(
                    headers,
                    call.GetStatus,
                    call.GetTrailers);
            }

            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                yield return call.ResponseStream.Current.GetValue1();
            }

            if (context != null && !token.IsCancellationRequested)
            {
                context.ServerResponse = new ServerResponse(
                    context.ResponseHeaders!,
                    call.GetStatus(),
                    call.GetTrailers());
            }
        }
    }

    private static async ValueTask FilterLastAsync(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var request = (TRequest)contextInternal.RequestInternal.GetRaw().Request!;
        var method = (GrpcMethod<Message, TRequest, TResponseHeader, TResponse>)contextInternal.Method;
        var callOptions = contextInternal.CallOptions;

        var call = contextInternal.CallInvoker.AsyncServerStreamingCall(method, null, callOptions, request);

        var header = await ReadResponseHeaderAsync(call, method.ResponseHeaderMarshaller, contextInternal.CallContext, callOptions.CancellationToken).ConfigureAwait(false);
        var stream = ReadServerStreamAsync(call, contextInternal.CallContext, callOptions.CancellationToken);

        contextInternal.ResponseInternal.SetRaw(header, stream);
    }

    private static void FilterLast(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var request = (TRequest)contextInternal.RequestInternal.GetRaw().Request!;
        var method = (GrpcMethod<Message, TRequest, TResponseHeader, TResponse>)contextInternal.Method;
        var callOptions = contextInternal.CallOptions;

        var call = contextInternal.CallInvoker.AsyncServerStreamingCall(method, null, callOptions, request);
        var stream = ReadServerStreamAsync(call, contextInternal.CallContext, callOptions.CancellationToken);

        contextInternal.ResponseInternal.SetRaw(null, stream);
    }

    private IClientCallFilterHandler? CreateFilter(TRequest request)
    {
        var filter = _filterHandlerFactory?.CreateAsyncHandler(_method, _callInvoker, _callOptions);
        if (filter != null)
        {
            var contextInternal = (IClientFilterContextInternal)filter.Context;
            contextInternal.RequestInternal.SetRaw(request, null);
            contextInternal.CallContext = _callContext;
        }

        return filter;
    }
}