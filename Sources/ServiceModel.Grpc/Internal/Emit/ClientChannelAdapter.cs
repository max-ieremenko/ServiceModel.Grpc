using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ClientChannelAdapter
    {
        public static async Task AsyncUnaryCallWait(AsyncUnaryCall<Message> call, CallContext context)
        {
            using (call)
            {
                Metadata headers = default;
                if (context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                await call;

                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        public static async Task<T> GetAsyncUnaryCallResult<T>(AsyncUnaryCall<Message<T>> call, CallContext context)
        {
            Message<T> result;
            using (call)
            {
                Metadata headers = default;
                if (context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                result = await call;

                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        public static async IAsyncEnumerable<T> GetServerStreamingCallResult<T>(
            AsyncServerStreamingCall<Message<T>> call,
            CallContext context,
            [EnumeratorCancellation] CancellationToken token)
        {
            using (call)
            {
                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                while (await call.ResponseStream.MoveNext(token))
                {
                    yield return call.ResponseStream.Current.Value1;
                }

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        public static async Task WriteClientStreamingRequestWait<TRequest>(
            AsyncClientStreamingCall<Message<TRequest>, Message> call,
            IAsyncEnumerable<TRequest> request,
            CallContext context,
            CancellationToken token)
        {
            using (call)
            {
                await foreach (var i in request.WithCancellation(token))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!token.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                Metadata headers = null;
                if (!token.IsCancellationRequested && context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                await call.ResponseAsync.ConfigureAwait(false);

                if (!token.IsCancellationRequested && context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        public static async Task<TResponse> WriteClientStreamingRequest<TRequest, TResponse>(
            AsyncClientStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext context,
            CancellationToken token)
        {
            Message<TResponse> result;
            using (call)
            {
                await foreach (var i in request.WithCancellation(token))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!token.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                Metadata headers = null;
                if (!token.IsCancellationRequested && context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                result = await call.ResponseAsync.ConfigureAwait(false);

                if (!token.IsCancellationRequested && context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        public static async IAsyncEnumerable<TResponse> GetDuplexCallResult<TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext context,
            [EnumeratorCancellation] CancellationToken token)
        {
            using (call)
            {
                var writer = DuplexCallWriteRequest(request, call.RequestStream, token);

                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                while (await call.ResponseStream.MoveNext(token))
                {
                    yield return call.ResponseStream.Current.Value1;
                }

                await writer.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders,
                        call.GetStatus(),
                        call.GetTrailers());
                }
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
