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
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding
{
    internal sealed class DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponseHeader, TResponse>
        where TRequestHeader : class
        where TResponseHeader : class
    {
        private readonly Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponse>)>> _invoker;
        private readonly Marshaller<TRequestHeader>? _requestHeaderMarshaller;
        private readonly Marshaller<TResponseHeader>? _responseHeaderMarshaller;

        public DuplexStreamingServerCallHandler(
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            Marshaller<TResponseHeader>? responseHeaderMarshaller)
        {
            _invoker = invoker;
            _requestHeaderMarshaller = requestHeaderMarshaller;
            _responseHeaderMarshaller = responseHeaderMarshaller;
        }

        public Task Handle(
            TService service,
            IAsyncStreamReader<Message<TRequest>> input,
            IServerStreamWriter<Message<TResponse>> output,
            ServerCallContext serverCallContext)
        {
            TRequestHeader? header = null;
            if (_requestHeaderMarshaller != null)
            {
                header = CompatibilityTools.DeserializeMethodInputHeader(_requestHeaderMarshaller, serverCallContext.RequestHeaders);
            }

            var request = ServerChannelAdapter.ReadClientStream(input, serverCallContext);

            var result = _invoker(service, header, request, serverCallContext);
            return ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, output, serverCallContext);
        }
    }
}