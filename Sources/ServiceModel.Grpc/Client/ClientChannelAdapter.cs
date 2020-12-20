// <copyright>
// Copyright 2020 Max Ieremenko
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Client
{
    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ClientChannelAdapter
    {
        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="call">Single request-response call.</param>
        /// <param name="context">Optional call context.</param>
        /// <returns>Call result.</returns>
        public static async Task AsyncUnaryCallWait(AsyncUnaryCall<Message> call, CallContext? context)
        {
            using (call)
            {
                Metadata? headers = default;
                if (context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                await call;

                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="call">Single request-response call.</param>
        /// <param name="context">Optional call context.</param>
        /// <returns>Call result.</returns>
        public static async Task<T> GetAsyncUnaryCallResult<T>(AsyncUnaryCall<Message<T>> call, CallContext? context)
        {
            Message<T> result;
            using (call)
            {
                Metadata? headers = default;
                if (context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                result = await call;

                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="call">Single request-response call.</param>
        /// <param name="context">Optional call context.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Call result.</returns>
        public static async IAsyncEnumerable<T> GetServerStreamingCallResult<T>(
            AsyncServerStreamingCall<Message<T>> call,
            CallContext? context,
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

                while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
                {
                    yield return call.ResponseStream.Current.Value1;
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

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <param name="call">Single request-response call.</param>
        /// <param name="request">The call request.</param>
        /// <param name="context">Optional call context.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Call result.</returns>
        public static async Task WriteClientStreamingRequestWait<TRequest>(
            AsyncClientStreamingCall<Message<TRequest>, Message> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            using (call)
            {
                await foreach (var i in request.WithCancellation(token).ConfigureAwait(false))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!token.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                Metadata? headers = null;
                if (!token.IsCancellationRequested && context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                await call.ResponseAsync.ConfigureAwait(false);

                if (!token.IsCancellationRequested && context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="call">Single request-response call.</param>
        /// <param name="request">The call request.</param>
        /// <param name="context">Optional call context.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Call result.</returns>
        public static async Task<TResponse> WriteClientStreamingRequest<TRequest, TResponse>(
            AsyncClientStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            Message<TResponse> result;
            using (call)
            {
                await foreach (var i in request.WithCancellation(token).ConfigureAwait(false))
                {
                    await call.RequestStream.WriteAsync(new Message<TRequest>(i)).ConfigureAwait(false);
                }

                if (!token.IsCancellationRequested)
                {
                    await call.RequestStream.CompleteAsync().ConfigureAwait(false);
                }

                Metadata? headers = null;
                if (!token.IsCancellationRequested && context != null)
                {
                    headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                }

                result = await call.ResponseAsync.ConfigureAwait(false);

                if (!token.IsCancellationRequested && context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="call">Single request-response call.</param>
        /// <param name="request">The call request.</param>
        /// <param name="context">Optional call context.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Call result.</returns>
        public static async IAsyncEnumerable<TResponse> GetDuplexCallResult<TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
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

                while (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
                {
                    yield return call.ResponseStream.Current.Value1;
                }

                await writer.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
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
            await foreach (var i in request.WithCancellation(token).ConfigureAwait(false))
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
