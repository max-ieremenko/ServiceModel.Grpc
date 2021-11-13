﻿// <copyright>
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
    internal sealed class ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponse>
        where TRequestHeader : class
        where TResponse : class
    {
        private readonly Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> _invoker;
        private readonly Marshaller<TRequestHeader>? _requestHeaderMarshaller;

        public ClientStreamingServerCallHandler(
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller)
        {
            _invoker = invoker;
            _requestHeaderMarshaller = requestHeaderMarshaller;
        }

        public Task<TResponse> Handle(
            TService service,
            IAsyncStreamReader<Message<TRequest>> stream,
            ServerCallContext serverCallContext)
        {
            TRequestHeader? header = null;
            if (_requestHeaderMarshaller != null)
            {
                header = CompatibilityTools.DeserializeMethodInputHeader(_requestHeaderMarshaller, serverCallContext.RequestHeaders);
            }

            var request = ServerChannelAdapter.ReadClientStream(stream, serverCallContext);

            return _invoker(service, header, request, serverCallContext);
        }
    }
}