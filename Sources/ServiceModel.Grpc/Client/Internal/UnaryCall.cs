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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Filters.Internal;

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
public readonly ref struct UnaryCall<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    //// ReSharper disable StaticMemberInGenericType
    private static readonly Action<IClientFilterContext> BlockingFilterLast = FilterLast;
    private static readonly Func<IClientFilterContext, ValueTask> AsyncFilterLast = FilterLastAsync;
    //// ReSharper restore StaticMemberInGenericType

    private readonly Method<TRequest, TResponse> _method;
    private readonly CallInvoker _callInvoker;
    private readonly IClientCallFilterHandlerFactory? _filterHandlerFactory;

    private readonly CallOptions _callOptions;
    private readonly CallContext? _callContext;

    public UnaryCall(
        Method<TRequest, TResponse> method,
        CallInvoker callInvoker,
        in CallOptionsBuilder callOptionsBuilder,
        IClientCallFilterHandlerFactory? filterHandlerFactory)
    {
        _method = method;
        _callInvoker = callInvoker;
        _filterHandlerFactory = filterHandlerFactory;

        _callContext = callOptionsBuilder.CallContext;
        _callOptions = callOptionsBuilder.Build();
    }

    public void Invoke(TRequest request) => InvokeCore(request);

    public TResult Invoke<TResult>(TRequest request)
    {
        var result = InvokeCore(request);
        return ((Message<TResult>)result).Value1;
    }

    public Task InvokeAsync(TRequest request) => InvokeCoreAsync(request);

    public Task<TResult> InvokeAsync<TResult>(TRequest request)
    {
        var responseTask = InvokeCoreAsync(request);
        return AdaptResultAsync<TResult>(responseTask);
    }

    internal static async Task<TResponse> CallAsync(AsyncUnaryCall<TResponse> call, CallContext? callContext, CancellationToken token)
    {
        TResponse response;
        using (call)
        {
            if (callContext != null && !token.IsCancellationRequested)
            {
                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                callContext.ServerResponse = new ServerResponse(
                    headers,
                    call.GetStatus,
                    call.GetTrailers);
            }

            response = await call.ResponseAsync.ConfigureAwait(false);

            if (callContext != null && !token.IsCancellationRequested)
            {
                callContext.ServerResponse = new ServerResponse(
                    callContext.ResponseHeaders!,
                    call.GetStatus(),
                    call.GetTrailers());
            }
        }

        return response;
    }

    private static async Task<TResponse> CallWithFilterAsync(IClientCallFilterHandler filter)
    {
        await filter.InvokeAsync(AsyncFilterLast).ConfigureAwait(false);

        var response = ((IClientFilterContextInternal)filter.Context).ResponseInternal.GetRaw().Response;

        return (TResponse)response;
    }

    private static async Task<TResult> AdaptResultAsync<TResult>(Task<TResponse> responseTask)
    {
        object response = await responseTask.ConfigureAwait(false);
        var result = (Message<TResult>)response;
        return result.Value1;
    }

    private static void FilterLast(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var method = (Method<TRequest, TResponse>)contextInternal.Method;
        var request = (TRequest)contextInternal.RequestInternal.GetRaw().Request!;

        var response = contextInternal.CallInvoker.BlockingUnaryCall(method, null, contextInternal.CallOptions, request);

        contextInternal.ResponseInternal.SetRaw(response, null);
    }

    private static async ValueTask FilterLastAsync(IClientFilterContext context)
    {
        var contextInternal = (IClientFilterContextInternal)context;
        var method = (Method<TRequest, TResponse>)context.Method;
        var request = (TRequest)contextInternal.RequestInternal.GetRaw().Request!;

        var call = contextInternal.CallInvoker.AsyncUnaryCall(method, null, contextInternal.CallOptions, request);
        var response = await CallAsync(call, contextInternal.CallContext, contextInternal.CallOptions.CancellationToken).ConfigureAwait(false);

        contextInternal.ResponseInternal.SetRaw(response, null);
    }

    private object InvokeCore(TRequest request)
    {
        var filter = _filterHandlerFactory?.CreateBlockingHandler(_method, _callInvoker, _callOptions);
        if (filter == null)
        {
            return _callInvoker.BlockingUnaryCall(_method, null, _callOptions, request);
        }

        var contextInternal = (IClientFilterContextInternal)filter.Context;
        contextInternal.RequestInternal.SetRaw(request, null);
        filter.Invoke(BlockingFilterLast);

        return contextInternal.ResponseInternal.GetRaw().Response;
    }

    private Task<TResponse> InvokeCoreAsync(TRequest request)
    {
        var filter = _filterHandlerFactory?.CreateAsyncHandler(_method, _callInvoker, _callOptions);

        if (filter == null)
        {
            var call = _callInvoker.AsyncUnaryCall(_method, null, _callOptions, request);
            return CallAsync(call, _callContext, _callOptions.CancellationToken);
        }

        var contextInternal = (IClientFilterContextInternal)filter.Context;
        contextInternal.RequestInternal.SetRaw(request, null);
        contextInternal.CallContext = _callContext;
        return CallWithFilterAsync(filter);
    }
}