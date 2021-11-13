// <copyright>
// Copyright 2021 Max Ieremenko
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
    internal sealed class ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse>
        where TRequest : class
        where TResponseHeader : class
    {
        private readonly Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponse>)>> _invoker;
        private readonly Marshaller<TResponseHeader>? _responseHeaderMarshaller;

        public ServerStreamingServerCallHandler(
            Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> invoker,
            Marshaller<TResponseHeader>? responseHeaderMarshaller)
        {
            _invoker = invoker;
            _responseHeaderMarshaller = responseHeaderMarshaller;
        }

        public Task Handle(TService service, TRequest request, IServerStreamWriter<Message<TResponse>> stream, ServerCallContext serverCallContext)
        {
            var result = _invoker(service, request, serverCallContext);
            return ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, stream, serverCallContext);
        }
    }
}