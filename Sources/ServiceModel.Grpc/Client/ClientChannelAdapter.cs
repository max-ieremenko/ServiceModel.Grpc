// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented

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
        /// <exclude />
        public static async Task AsyncUnaryCallWait(AsyncUnaryCall<Message> call, CallContext? context, CancellationToken token)
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

                await call.ResponseAsync.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        /// <exclude />
        public static async Task<T> GetAsyncUnaryCallResult<T>(AsyncUnaryCall<Message<T>> call, CallContext? context, CancellationToken token)
        {
            Message<T> result;
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

                result = await call.ResponseAsync.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        /// <exclude />
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

        /// <exclude />
        public static async Task<IAsyncEnumerable<T>> GetServerStreamingCallResultAsync<T>(
            AsyncServerStreamingCall<Message<T>> call,
            CallContext? context,
            CancellationToken token)
        {
            try
            {
                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }
            }
            catch
            {
                call.Dispose();
                throw;
            }

            return ReadServerStreamingCallResultAsync(call, context, token);
        }

        /// <exclude />
        public static async Task<(THeader Header, IAsyncEnumerable<TResult> Stream)> GetServerStreamingCallResultAsync<THeader, TResult>(
            AsyncServerStreamingCall<Message<TResult>> call,
            CallContext? context,
            CancellationToken token,
            Marshaller<THeader> marshaller)
        {
            THeader header;
            try
            {
                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                header = CompatibilityTools.DeserializeMethodOutputHeader(marshaller, headers);
            }
            catch
            {
                call.Dispose();
                throw;
            }

            var stream = ReadServerStreamingCallResultAsync(call, context, token);
            return (header, stream);
        }

        /// <exclude />
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

                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                await call.ResponseAsync.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }
        }

        /// <exclude />
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

                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                result = await call.ResponseAsync.ConfigureAwait(false);

                if (!token.IsCancellationRequested && context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
                        call.GetStatus(),
                        call.GetTrailers());
                }
            }

            return result.Value1;
        }

        /// <exclude />
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

        /// <exclude />
        public static async Task<IAsyncEnumerable<TResponse>> GetDuplexCallResultAsync<TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            Task writer;
            try
            {
                writer = DuplexCallWriteRequest(request, call.RequestStream, token);

                if (context != null && !token.IsCancellationRequested)
                {
                    var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }
            }
            catch
            {
                call.Dispose();
                throw;
            }

            return ReadDuplexCallResultAsync(call, context, writer, token);
        }

        /// <exclude />
        public static async Task<(THeader Header, IAsyncEnumerable<TResponse> Stream)> GetDuplexCallResultAsync<THeader, TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token,
            Marshaller<THeader> marshaller)
        {
            Task writer;
            THeader header;
            try
            {
                writer = DuplexCallWriteRequest(request, call.RequestStream, token);
                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                header = CompatibilityTools.DeserializeMethodOutputHeader(marshaller, headers);
            }
            catch
            {
                call.Dispose();
                throw;
            }

            var stream = ReadDuplexCallResultAsync(call, context, writer, token);
            return (header, stream);
        }

        public static async Task<TNewResult> ContinueWith<TResult, TNewResult>(Task<TResult> task, Func<TResult, TNewResult> continuationFunction)
        {
            var result = await task.ConfigureAwait(false);
            return continuationFunction(result);
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

        private static async IAsyncEnumerable<T> ReadServerStreamingCallResultAsync<T>(
            AsyncServerStreamingCall<Message<T>> call,
            CallContext? context,
            [EnumeratorCancellation] CancellationToken token)
        {
            using (call)
            {
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

        private static async IAsyncEnumerable<TResponse> ReadDuplexCallResultAsync<TRequest, TResponse>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            CallContext? context,
            Task writer,
            [EnumeratorCancellation] CancellationToken token)
        {
            using (call)
            {
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
    }
}
