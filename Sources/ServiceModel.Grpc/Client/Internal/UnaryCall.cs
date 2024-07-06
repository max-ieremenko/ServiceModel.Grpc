// <copyright>
// Copyright Max Ieremenko
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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.Internal;

internal readonly ref struct UnaryCall<TRequest, TResponse>
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
        IMethod method,
        CallInvoker callInvoker,
        in CallOptionsBuilder callOptionsBuilder,
        IClientCallFilterHandlerFactory? filterHandlerFactory)
    {
        _method = (Method<TRequest, TResponse>)method;
        _callInvoker = callInvoker;
        _filterHandlerFactory = filterHandlerFactory;

        _callContext = callOptionsBuilder.CallContext;
        _callOptions = callOptionsBuilder.Build();
    }

    public void Invoke(TRequest request) => InvokeCore(request);

    public TResult? Invoke<TResult>(TRequest request)
    {
        var result = InvokeCore(request);
        return ((IMessage<TResult>)result).GetValue1();
    }

    public Task InvokeAsync(TRequest request) => InvokeCoreAsync(request);

    public Task<TResult?> InvokeAsync<TResult>(TRequest request)
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
                CallContextExtensions.SetResponse(callContext, headers, call.GetStatus, call.GetTrailers);
            }

            response = await call.ResponseAsync.ConfigureAwait(false);

            if (callContext != null && !token.IsCancellationRequested)
            {
                CallContextExtensions.SetResponse(callContext, callContext.ResponseHeaders!, call.GetStatus(), call.GetTrailers());
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

    private static async Task<TResult?> AdaptResultAsync<TResult>(Task<TResponse> responseTask)
    {
        var response = await responseTask.ConfigureAwait(false);
        var result = (IMessage<TResult>)response;
        return result.GetValue1();
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