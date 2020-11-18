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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class AspNetCoreServiceMethodBinder<TService> : IServiceMethodBinder<TService>
        where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;

        public AspNetCoreServiceMethodBinder(
            ServiceMethodProviderContext<TService> context,
            IMarshallerFactory marshallerFactory)
        {
            _context = context;
            MarshallerFactory = marshallerFactory;
        }

        public IMarshallerFactory MarshallerFactory { get; }

        public bool RequiresMetadata => true;

        public void AddUnaryMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = handler.Method.CreateDelegate<UnaryServerMethod<TService, TRequest, TResponse>>(handler.Target);
            _context.AddUnaryMethod(method, metadata, invoker);
        }

        public void AddClientStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, IAsyncStreamReader<TRequest>, ServerCallContext, Task<TResponse>> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = handler.Method.CreateDelegate<ClientStreamingServerMethod<TService, TRequest, TResponse>>(handler.Target);
            _context.AddClientStreamingMethod(method, metadata, invoker);
        }

        public void AddServerStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, TRequest, IServerStreamWriter<TResponse>, ServerCallContext, Task> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = handler.Method.CreateDelegate<ServerStreamingServerMethod<TService, TRequest, TResponse>>(handler.Target);
            _context.AddServerStreamingMethod(method, metadata, invoker);
        }

        public void AddDuplexStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, Task> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = handler.Method.CreateDelegate<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(handler.Target);
            _context.AddDuplexStreamingMethod(method, metadata, invoker);
        }
    }
}
