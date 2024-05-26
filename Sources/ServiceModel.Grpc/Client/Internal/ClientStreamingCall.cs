// <copyright>
// Copyright 2022 Max Ieremenko
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
public readonly ref struct ClientStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponse>
    where TRequestHeader : class
    where TRequest : class, IMessage<TRequestValue>, new()
    where TResponse : class
{
    //// ReSharper disable StaticMemberInGenericType
    private static readonly Func<IClientFilterContext, ValueTask> AsyncFilterLast = FilterLastAsync;
    //// ReSharper restore StaticMemberInGenericType

    private readonly GrpcMethod<TRequestHeader, TRequest, Message, TResponse> _method;
    private readonly TRequestHeader? _requestHeader;
    private readonly CallInvoker _callInvoker;
    private readonly CallOptions _callOptions;
    private readonly IClientCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly CallContext? _callContext;

    public ClientStreamingCall(
        IMethod method,
        CallInvoker callInvoker,
        in CallOptionsBuilder callOptionsBuilder,
        IClientCallFilterHandlerFactory? filterHandlerFactory,
        TRequestHeader? requestHeader)
    {
        _method = (GrpcMethod<TRequestHeader, TRequest, Message, TResponse>)method;
        _requestHeader = requestHeader;
        _callInvoker = callInvoker;
        _filterHandlerFactory = filterHandlerFactory;

        _callContext = callOptionsBuilder.CallContext;
        _callOptions = callOptionsBuilder.Build();
    }

    public Task InvokeAsync(IAsyncEnumerable<TRequestValue> request) => InvokeCoreAsync(request);

    public Task<TResult?> InvokeAsync<TResult>(IAsyncEnumerable<TRequestValue> request)
    {
        var responseTask = InvokeCoreAsync(request);
        return AdaptResultAsync<TResult>(responseTask);
    }

    private static async Task<TResponse> CallAsync(
        AsyncClientStreamingCall<TRequest, TResponse> call,
        IAsyncEnumerable<TRequestValue?> request,
        CallContext? context,
        CancellationToken token)
    {
        TResponse response;
        using (call)
        using (var writer = new ClientStreamWriter<TRequest, TRequestValue>(request, call.RequestStream, token))
        {
            if (context != null && !token.IsCancellationRequested)
            {
                context.TraceClientStreaming?.Invoke(writer.Task);

                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                context.ServerResponse = new ServerResponse(
                    headers,
                    call.GetStatus,
                    call.GetTrailers);
            }

            response = await call.ResponseAsync.ConfigureAwait(false);

            if (!token.IsCancellationRequested && context != null)
            {
                context.ServerResponse = new ServerResponse(
                    context.ResponseHeaders!,
                    call.GetStatus(),
                    call.GetTrailers());
            }

            await writer.WaitAsync(token).ConfigureAwait(false);
        }

        return response;
    }

    private static async Task<TResult?> AdaptResultAsync<TResult>(Task<TResponse> responseTask)
    {
        object response = await responseTask.ConfigureAwait(false);
        var result = (IMessage<TResult>)response;
        return result.GetValue1();
    }

    private static async Task<TResponse> CallWithFilterAsync(IClientCallFilterHandler filter)
    {
        await filter.InvokeAsync(AsyncFilterLast).ConfigureAwait(false);

        var response = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw().Response;

        return (TResponse)response;
    }

    private static async ValueTask FilterLastAsync(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var method = (GrpcMethod<TRequestHeader, TRequest, Message, TResponse>)context.Method;
        var request = contextInternal.RequestInternal.GetRaw();

        var callOptions = ClientChannelAdapter.AddRequestHeader(
            contextInternal.CallOptions,
            method.RequestHeaderMarshaller,
            (TRequestHeader?)request.Request);

        var call = contextInternal.CallInvoker.AsyncClientStreamingCall(method, null, callOptions);
        var response = await CallAsync(
                call,
                (IAsyncEnumerable<TRequestValue?>)request.Stream!,
                contextInternal.CallContext,
                callOptions.CancellationToken)
            .ConfigureAwait(false);

        contextInternal.ResponseInternal.SetRaw(response, null);
    }

    private Task<TResponse> InvokeCoreAsync(IAsyncEnumerable<TRequestValue> request)
    {
        var filter = _filterHandlerFactory?.CreateBlockingHandler(_method, _callInvoker, _callOptions);

        if (filter == null)
        {
            var callOptions = ClientChannelAdapter.AddRequestHeader(_callOptions, _method.RequestHeaderMarshaller, _requestHeader);
            var call = _callInvoker.AsyncClientStreamingCall(_method, null, callOptions);
            return CallAsync(call, request, _callContext, callOptions.CancellationToken);
        }

        var contextInternal = (IClientFilterContextInternal)filter.Context;
        contextInternal.RequestInternal.SetRaw(_requestHeader, request);
        contextInternal.CallContext = _callContext;
        ////contextInternal.RequestHeaderMarshaller = _headerMarshaller;
        return CallWithFilterAsync(filter);
    }
}