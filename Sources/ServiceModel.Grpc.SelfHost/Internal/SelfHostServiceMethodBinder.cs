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
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class SelfHostServiceMethodBinder<TService> : IServiceMethodBinder<TService>
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ServerServiceDefinition.Builder _builder;

        public SelfHostServiceMethodBinder(
            IMarshallerFactory marshallerFactory,
            Func<TService> serviceFactory,
            ServerServiceDefinition.Builder builder)
        {
            MarshallerFactory = marshallerFactory;
            _serviceFactory = serviceFactory;
            _builder = builder;
        }

        public IMarshallerFactory MarshallerFactory { get; }

        public bool RequiresMetadata => false;

        public void AddUnaryMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler);
            _builder.AddMethod(method, invoker.Handle);
        }

        public void AddClientStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, IAsyncStreamReader<TRequest>, ServerCallContext, Task<TResponse>> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = new ClientStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler);
            _builder.AddMethod(method, invoker.Handle);
        }

        public void AddServerStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, TRequest, IServerStreamWriter<TResponse>, ServerCallContext, Task> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = new ServerStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler);
            _builder.AddMethod(method, invoker.Handle);
        }

        public void AddDuplexStreamingMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            IList<object> metadata,
            Func<TService, IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, Task> handler)
            where TRequest : class
            where TResponse : class
        {
            var invoker = new DuplexStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler);
            _builder.AddMethod(method, invoker.Handle);
        }
    }
}
