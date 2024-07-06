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

using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal sealed class ClientNativeInterceptor : Interceptor
{
    public ClientNativeInterceptor(IClientCallInterceptor callInterceptor)
    {
        CallInterceptor = GrpcPreconditions.CheckNotNull(callInterceptor, nameof(callInterceptor));
    }

    internal IClientCallInterceptor CallInterceptor { get; }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        try
        {
            return base.BlockingUnaryCall(request, context, continuation);
        }
        catch (RpcException ex)
        {
            CallInterceptor.OnError(new ClientCallInterceptorContext(context.Options, context.Host, context.Method), ex);
            throw;
        }
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var callContext = new ClientCallInterceptorContext(context.Options, context.Host, context.Method);

        var call = base.AsyncUnaryCall(request, context, continuation);

        return new AsyncUnaryCall<TResponse>(
            WaitAsyncUnaryCall(callContext, call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var callContext = new ClientCallInterceptorContext(context.Options, context.Host, context.Method);

        var call = base.AsyncClientStreamingCall(context, continuation);

        return new AsyncClientStreamingCall<TRequest, TResponse>(
            new ClientStreamWriterInterceptor<TRequest>(call.RequestStream, callContext, CallInterceptor),
            WaitAsyncUnaryCall(callContext, call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var callContext = new ClientCallInterceptorContext(context.Options, context.Host, context.Method);

        var call = base.AsyncServerStreamingCall(request, context, continuation);

        return new AsyncServerStreamingCall<TResponse>(
            new AsyncStreamReaderInterceptor<TResponse>(call.ResponseStream, callContext, CallInterceptor),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var callContext = new ClientCallInterceptorContext(context.Options, context.Host, context.Method);

        var call = base.AsyncDuplexStreamingCall(context, continuation);

        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            new ClientStreamWriterInterceptor<TRequest>(call.RequestStream, callContext, CallInterceptor),
            new AsyncStreamReaderInterceptor<TResponse>(call.ResponseStream, callContext, CallInterceptor),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> WaitAsyncUnaryCall<TResponse>(
        ClientCallInterceptorContext context,
        Task<TResponse> responseAsync)
        where TResponse : class
    {
        try
        {
            return await responseAsync.ConfigureAwait(false);
        }
        catch (RpcException ex)
        {
            CallInterceptor.OnError(context, ex);
            throw;
        }
    }
}