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

using System.Collections.Generic;
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
    public ref struct ClientStreamingCall<TRequestHeader, TRequest, TResponse>
        where TRequestHeader : class
        where TResponse : class
    {
        private readonly Method<Message<TRequest>, TResponse> _method;
        private readonly CallInvoker _callInvoker;

        private readonly CallContext? _callContext;
        private CallOptions _callOptions;

        public ClientStreamingCall(
            Method<Message<TRequest>, TResponse> method,
            CallInvoker callInvoker,
            in CallOptionsBuilder callOptionsBuilder)
        {
            _method = method;
            _callInvoker = callInvoker;

            _callContext = callOptionsBuilder.CallContext;
            _callOptions = callOptionsBuilder.Build();
        }

        public ClientStreamingCall<TRequestHeader, TRequest, TResponse> WithRequestHeader(
            Marshaller<TRequestHeader> marshaller,
            TRequestHeader header)
        {
            var metadata = CompatibilityTools.SerializeMethodInputHeader(marshaller, header);
            _callOptions = CallOptionsBuilder.MergeCallOptions(_callOptions, new CallOptions(metadata));
            return this;
        }

        public Task InvokeAsync(IAsyncEnumerable<TRequest> request)
        {
            object call = _callInvoker.AsyncClientStreamingCall(_method, null, _callOptions);
            var typedCall = (AsyncClientStreamingCall<Message<TRequest>, Message>)call;

            return CallAsync(typedCall, request, _callContext, _callOptions.CancellationToken);
        }

        public Task<TResult> InvokeAsync<TResult>(IAsyncEnumerable<TRequest> request)
        {
            object call = _callInvoker.AsyncClientStreamingCall(_method, null, _callOptions);
            var typedCall = (AsyncClientStreamingCall<Message<TRequest>, Message<TResult>>)call;

            return CallAsync(typedCall, request, _callContext, _callOptions.CancellationToken);
        }

        private static async Task CallAsync(
            AsyncClientStreamingCall<Message<TRequest>, Message> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            using (call)
            {
                _ = ClientChannelAdapter.WriteClientStream(request, call.RequestStream, token);

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

        private static async Task<TResult> CallAsync<TResult>(
            AsyncClientStreamingCall<Message<TRequest>, Message<TResult>> call,
            IAsyncEnumerable<TRequest> request,
            CallContext? context,
            CancellationToken token)
        {
            Message<TResult> result;
            using (call)
            {
                _ = ClientChannelAdapter.WriteClientStream(request, call.RequestStream, token);

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
    }
}