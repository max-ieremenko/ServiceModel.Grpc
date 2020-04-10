using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ClientChannelAdapter
    {
        public static CallOptions Combine() => default;

        public static CallOptions Combine(CallOptions callOptions) => callOptions;

        public static CallOptions Combine(CallContext callContext) => callContext.ClientCallContext;

        public static CallOptions Combine(ServerCallContext callContext)
        {
            if (callContext != null)
            {
                throw new NotSupportedException("ServerCallContext can be propagated from client call.");
            }

            return default;
        }

        public static CallOptions Combine(CancellationToken token)
        {
            return default(CallOptions).WithCancellationToken(token);
        }

        public static async Task AsyncUnaryCallWait(AsyncUnaryCall<Message> call)
        {
            await call;
        }

        public static async Task<T> GetAsyncUnaryCallResult<T>(AsyncUnaryCall<Message<T>> call)
        {
            var result = await call;
            return result.Value1;
        }

        public static async IAsyncEnumerable<T> GetServerStreamingCallResult<T>(AsyncServerStreamingCall<Message<T>> call, CallOptions options)
        {
            using (call)
            {
                await call.ResponseHeadersAsync.ConfigureAwait(false);

                //// TODO: call.GetTrailers();
                //// InvalidOperationException : Can't get the call trailers because the call has not completed successfully.

                while (await call.ResponseStream.MoveNext(options.CancellationToken))
                {
                    yield return call.ResponseStream.Current.Value1;
                }
            }
        }

        public static async Task WriteClientStreamingRequestWait<TRequest>(
            AsyncClientStreamingCall<Message<TRequest>, Message> call,
            IAsyncEnumerable<TRequest> request,
            CallOptions options)
        {
            using (call)
            {
                await foreach (var i in request.WithCancellation(options.CancellationToken))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!options.CancellationToken.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                await call.ResponseAsync.ConfigureAwait(false);
            }
        }

        public static async Task<TResponse> WriteClientStreamingRequest<TRequest, TResponse>(
            AsyncClientStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallOptions options)
        {
            using (call)
            {
                await foreach (var i in request.WithCancellation(options.CancellationToken))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!options.CancellationToken.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                var result = await call.ResponseAsync.ConfigureAwait(false);
                return result.Value1;
            }
        }

        public static async IAsyncEnumerable<TResponse> GetDuplexCallResult<TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallOptions options)
        {
            using (call)
            {
                var writer = DuplexCallWriteRequest(request, call.RequestStream, options.CancellationToken);

                await call.ResponseHeadersAsync.ConfigureAwait(false);

                //// TODO: call.GetTrailers();
                //// InvalidOperationException : Can't get the call trailers because the call has not completed successfully.

                while (await call.ResponseStream.MoveNext(options.CancellationToken))
                {
                    yield return call.ResponseStream.Current.Value1;
                }

                await writer.ConfigureAwait(false);
            }
        }

        private static async Task DuplexCallWriteRequest<TRequest>(
            IAsyncEnumerable<TRequest> request,
            IClientStreamWriter<Message<TRequest>> stream,
            CancellationToken token)
        {
            await foreach (var i in request.WithCancellation(token))
            {
                await stream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
            }

            if (!token.IsCancellationRequested)
            {
                await stream.CompleteAsync().ConfigureAwait(false);
            }
        }
    }
}
