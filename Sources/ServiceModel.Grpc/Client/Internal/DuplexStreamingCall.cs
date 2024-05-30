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
public readonly ref struct DuplexStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>
    where TRequestHeader : class
    where TRequest : class, IMessage<TRequestValue>, new()
    where TResponseHeader : class
    where TResponse : class, IMessage<TResponseValue>
{
    //// ReSharper disable StaticMemberInGenericType
    private static readonly Action<IClientFilterContext> BlockingFilterLast = FilterLast;
    private static readonly Func<IClientFilterContext, ValueTask> AsyncFilterLast = FilterLastAsync;
    private static readonly Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, IAsyncEnumerable<TResponseValue?>> GetStream = (_, stream) => stream;
    //// ReSharper restore StaticMemberInGenericType

    private readonly GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse> _method;
    private readonly TRequestHeader? _requestHeader;
    private readonly CallInvoker _callInvoker;
    private readonly CallOptions _callOptions;
    private readonly IClientCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly CallContext? _callContext;

    public DuplexStreamingCall(
        IMethod method,
        CallInvoker callInvoker,
        in CallOptionsBuilder callOptionsBuilder,
        IClientCallFilterHandlerFactory? filterHandlerFactory,
        TRequestHeader? requestHeader)
    {
        _method = (GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>)method;
        _requestHeader = requestHeader;
        _callInvoker = callInvoker;
        _filterHandlerFactory = filterHandlerFactory;

        _callContext = callOptionsBuilder.CallContext;
        _callOptions = callOptionsBuilder.Build();
    }

    public IAsyncEnumerable<TResponseValue?> Invoke(IAsyncEnumerable<TRequestValue?> request)
    {
        var filter = CreateFilter(request);

        IAsyncEnumerable<TResponseValue?> result;
        if (filter == null)
        {
            var callOptions = ClientChannelAdapter.AddRequestHeader(_callOptions, _method.RequestHeaderMarshaller, _requestHeader);
            var call = _callInvoker.AsyncDuplexStreamingCall(_method, null, callOptions);
            result = InvokeCore(call, request, _callContext, callOptions.CancellationToken);
        }
        else
        {
            filter.Invoke(BlockingFilterLast);
            var stream = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw().Stream;
            result = (IAsyncEnumerable<TResponseValue?>)stream!;
        }

        return result;
    }

    public Task<IAsyncEnumerable<TResponseValue?>> InvokeAsync(IAsyncEnumerable<TRequestValue?> request) => InvokeAsync(request, GetStream);

    public Task<TResult> InvokeAsync<TResult>(
        IAsyncEnumerable<TRequestValue?> request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
    {
        var filter = CreateFilter(request);
        if (filter == null)
        {
            var callOptions = ClientChannelAdapter.AddRequestHeader(_callOptions, _method.RequestHeaderMarshaller, _requestHeader);
            var call = _callInvoker.AsyncDuplexStreamingCall(_method, null, callOptions);
            return InvokeCoreAsync(call, request, _callContext, callOptions.CancellationToken, _method.ResponseHeaderMarshaller, continuationFunction);
        }

        return InvokeWithFilterAsync(filter, continuationFunction);
    }

    private static async Task<TResult> InvokeWithFilterAsync<TResult>(
        IClientCallFilterHandler filter,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue>, TResult> continuationFunction)
    {
        await filter.InvokeAsync(AsyncFilterLast).ConfigureAwait(false);

        var (responseHeader, response) = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw();
        var stream = (IAsyncEnumerable<TResponseValue>)response!;
        var header = (TResponseHeader?)responseHeader;

        return continuationFunction(header!, stream);
    }

    private static async Task<TResponseHeader?> ReadResponseHeaderAsync(
        AsyncDuplexStreamingCall<TRequest, TResponse> call,
        ClientStreamWriter<TRequest, TRequestValue> writer,
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
                    CallContextExtensions.SetResponse(context, headers, call.GetStatus, call.GetTrailers);
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
            writer.Dispose();
            throw;
        }

        return result;
    }

    private static IAsyncEnumerable<TResponseValue?> InvokeCore(
        AsyncDuplexStreamingCall<TRequest, TResponse> call,
        IAsyncEnumerable<TRequestValue?> request,
        CallContext? context,
        CancellationToken token)
    {
        ClientStreamWriter<TRequest, TRequestValue>? writer = null;
        try
        {
            writer = new ClientStreamWriter<TRequest, TRequestValue>(request, call.RequestStream, token);
            CallContextExtensions.TraceClientStreaming(context, writer.Task);
        }
        catch
        {
            call.Dispose();
            writer?.Dispose();
            throw;
        }

        return ReadServerStreamAsync(call, writer, context, token);
    }

    private static async Task<TResult> InvokeCoreAsync<TResult>(
        AsyncDuplexStreamingCall<TRequest, TResponse> call,
        IAsyncEnumerable<TRequestValue?> request,
        CallContext? context,
        CancellationToken token,
        Marshaller<TResponseHeader>? marshaller,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
    {
        ClientStreamWriter<TRequest, TRequestValue>? writer;
        try
        {
            writer = new ClientStreamWriter<TRequest, TRequestValue>(request, call.RequestStream, token);
            CallContextExtensions.TraceClientStreaming(context, writer.Task);
        }
        catch
        {
            call.Dispose();
            throw;
        }

        var header = await ReadResponseHeaderAsync(call, writer, marshaller, context, token).ConfigureAwait(false);
        var stream = ReadServerStreamAsync(call, writer, context, token);

        return continuationFunction(header!, stream);
    }

    private static async IAsyncEnumerable<TResponseValue?> ReadServerStreamAsync(
        AsyncDuplexStreamingCall<TRequest, TResponse> call,
        ClientStreamWriter<TRequest, TRequestValue> writer,
        CallContext? context,
        [EnumeratorCancellation] CancellationToken token)
    {
        using (call)
        using (writer)
        {
            if (context != null && !CallContextExtensions.ContainsResponse(context) && !token.IsCancellationRequested)
            {
                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                CallContextExtensions.SetResponse(context, headers, call.GetStatus, call.GetTrailers);
            }

            while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
            {
                yield return call.ResponseStream.Current.GetValue1();
            }

            if (context != null && !token.IsCancellationRequested)
            {
                CallContextExtensions.SetResponse(context, context.ResponseHeaders!, call.GetStatus(), call.GetTrailers());
            }

            await writer.WaitAsync(token).ConfigureAwait(false);
        }
    }

    private static void FilterLast(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var method = (GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>)contextInternal.Method;
        var request = contextInternal.RequestInternal.GetRaw();

        var callOptions = ClientChannelAdapter.AddRequestHeader(
            contextInternal.CallOptions,
            method.RequestHeaderMarshaller,
            (TRequestHeader?)request.Request);

        var call = contextInternal.CallInvoker.AsyncDuplexStreamingCall(method, null, callOptions);

        var stream = InvokeCore(
            call,
            (IAsyncEnumerable<TRequestValue?>)request.Stream!,
            contextInternal.CallContext,
            callOptions.CancellationToken);

        contextInternal.ResponseInternal.SetRaw(null, stream);
    }

    private static async ValueTask FilterLastAsync(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var method = (GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>)contextInternal.Method;
        var request = contextInternal.RequestInternal.GetRaw();

        var callOptions = ClientChannelAdapter.AddRequestHeader(
            contextInternal.CallOptions,
            method.RequestHeaderMarshaller,
            (TRequestHeader?)request.Request);

        var call = contextInternal.CallInvoker.AsyncDuplexStreamingCall(method, null, callOptions);

        ClientStreamWriter<TRequest, TRequestValue>? writer;
        try
        {
            writer = new ClientStreamWriter<TRequest, TRequestValue>((IAsyncEnumerable<TRequestValue?>)request.Stream!, call.RequestStream, callOptions.CancellationToken);
            CallContextExtensions.TraceClientStreaming(contextInternal.CallContext, writer.Task);
        }
        catch
        {
            call.Dispose();
            throw;
        }

        var header = await ReadResponseHeaderAsync(
                call,
                writer,
                method.ResponseHeaderMarshaller,
                contextInternal.CallContext,
                callOptions.CancellationToken)
            .ConfigureAwait(false);
        var stream = ReadServerStreamAsync(call, writer, contextInternal.CallContext, callOptions.CancellationToken);

        contextInternal.ResponseInternal.SetRaw(header, stream);
    }

    private IClientCallFilterHandler? CreateFilter(object request)
    {
        var filter = _filterHandlerFactory?.CreateAsyncHandler(_method, _callInvoker, _callOptions);
        if (filter != null)
        {
            var contextInternal = (IClientFilterContextInternal)filter.Context;
            contextInternal.RequestInternal.SetRaw(_requestHeader, request);
            contextInternal.CallContext = _callContext;
        }

        return filter;
    }
}