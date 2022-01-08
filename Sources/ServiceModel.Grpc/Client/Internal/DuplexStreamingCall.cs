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

namespace ServiceModel.Grpc.Client.Internal
{
    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    public ref struct DuplexStreamingCall<TRequestHeader, TRequest, TResponseHeader, TResponse>
        where TRequestHeader : class
        where TResponseHeader : class
    {
        private readonly Method<Message<TRequest>, Message<TResponse>> _method;
        private readonly CallInvoker _callInvoker;

        private readonly CallContext? _callContext;
        private CallOptions _callOptions;
        private Marshaller<TResponseHeader>? _responseHeaderMarshaller;

        public DuplexStreamingCall(
            Method<Message<TRequest>, Message<TResponse>> method,
            CallInvoker callInvoker,
            in CallOptionsBuilder callOptionsBuilder)
        {
            _method = method;
            _callInvoker = callInvoker;

            _callContext = callOptionsBuilder.CallContext;
            _callOptions = callOptionsBuilder.Build();
            _responseHeaderMarshaller = null;
        }

        public DuplexStreamingCall<TRequestHeader, TRequest, TResponseHeader, TResponse> WithRequestHeader(
            Marshaller<TRequestHeader> marshaller,
            TRequestHeader header)
        {
            var metadata = CompatibilityTools.SerializeMethodInputHeader(marshaller, header);
            _callOptions = CallOptionsBuilder.MergeCallOptions(_callOptions, new CallOptions(metadata));
            return this;
        }

        public DuplexStreamingCall<TRequestHeader, TRequest, TResponseHeader, TResponse> WithResponseHeader(
            Marshaller<TResponseHeader> marshaller)
        {
            _responseHeaderMarshaller = marshaller;
            return this;
        }

        public IAsyncEnumerable<TResponse> Invoke(IAsyncEnumerable<TRequest> request)
        {
            var call = _callInvoker.AsyncDuplexStreamingCall(_method, null, _callOptions);

            Task writer;
            try
            {
                writer = ClientChannelAdapter.WriteClientStream(request, call.RequestStream, _callOptions.CancellationToken);
            }
            catch
            {
                call.Dispose();
                throw;
            }

            return ReadServerStreamAsync(call, writer, _callContext, _callOptions.CancellationToken);
        }

        public Task<IAsyncEnumerable<TResponse>> InvokeAsync(IAsyncEnumerable<TRequest> request)
        {
            var call = _callInvoker.AsyncDuplexStreamingCall(_method, null, _callOptions);

            return CallAsync(call, request, _callContext, _callOptions.CancellationToken);
        }

        public Task<TResult> InvokeAsync<TResult>(
            IAsyncEnumerable<TRequest> request,
            Func<TResponseHeader, IAsyncEnumerable<TResponse>, TResult> continuationFunction)
        {
            var call = _callInvoker.AsyncDuplexStreamingCall(_method, null, _callOptions);

            return CallAsync(call, request, _callContext, _callOptions.CancellationToken, _responseHeaderMarshaller!, continuationFunction);
        }

        private static async Task<IAsyncEnumerable<TResponse>> CallAsync(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            Task writer;
            try
            {
                writer = ClientChannelAdapter.WriteClientStream(request, call.RequestStream, token);

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

            return ReadServerStreamAsync(call, writer, context, token);
        }

        private static async Task<TResult> CallAsync<TResult>(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token,
            Marshaller<TResponseHeader> marshaller,
            Func<TResponseHeader, IAsyncEnumerable<TResponse>, TResult> continuationFunction)
        {
            TResponseHeader header = default!;
            Task writer;
            try
            {
                writer = ClientChannelAdapter.WriteClientStream(request, call.RequestStream, token);

                var headers = await call.ResponseHeadersAsync.ConfigureAwait(false);
                if (context != null)
                {
                    context.ServerResponse = new ServerResponse(
                        headers,
                        call.GetStatus,
                        call.GetTrailers);
                }

                if (headers != null && headers.Count != 0)
                {
                    header = CompatibilityTools.DeserializeMethodOutputHeader(marshaller, headers);
                }
                else
                {
                    await ClientChannelAdapter.WaitForServerStreamExceptionAsync(call.ResponseStream, headers, marshaller, token).ConfigureAwait(false);
                }
            }
            catch
            {
                call.Dispose();
                throw;
            }

            var stream = ReadServerStreamAsync(call, writer, context, token);
            return continuationFunction(header, stream);
        }

        private static async IAsyncEnumerable<TResponse> ReadServerStreamAsync(
            AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>> call,
            Task writer,
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
                    yield return call.ResponseStream.Current.Value1;
                }

                if (context != null && !token.IsCancellationRequested)
                {
                    context.ServerResponse = new ServerResponse(
                        context.ResponseHeaders!,
                        call.GetStatus(),
                        call.GetTrailers());
                }

                await ClientChannelAdapter.WaitForWriter(writer, token).ConfigureAwait(false);
            }
        }
    }
}
