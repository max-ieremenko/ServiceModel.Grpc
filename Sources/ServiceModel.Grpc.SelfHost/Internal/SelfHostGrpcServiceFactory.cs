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
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class SelfHostGrpcServiceFactory<TService> : IServiceBinder
    {
        private readonly ILogger _logger;
        private readonly IMarshallerFactory? _marshallerFactory;
        private readonly Func<TService> _serviceFactory;
        private readonly ServerServiceDefinition.Builder _builder;

        public SelfHostGrpcServiceFactory(
            ILogger logger,
            IMarshallerFactory? marshallerFactory,
            Func<TService> serviceFactory,
            ServerServiceDefinition.Builder builder)
        {
            _logger = logger;
            _marshallerFactory = marshallerFactory;
            _serviceFactory = serviceFactory;
            _builder = builder;
        }

        public void Bind()
        {
            var generator = new EmitGenerator { Logger = _logger };
            generator.BindService<TService>(this, _marshallerFactory ?? DataContractMarshallerFactory.Default);
        }

        public void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<UnaryServerCallHandler<TService, TRequest, TResponse>.UnaryServerMethod>(callInfo.Channel);
            var handler = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        public void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<ClientStreamingServerCallHandler<TService, TRequest, TResponse>.ClientStreamingServerMethod>(callInfo.Channel);
            var handler = new ClientStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        public void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<ServerStreamingServerCallHandler<TService, TRequest, TResponse>.ServerStreamingServerMethod>(callInfo.Channel);
            var handler = new ServerStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        public void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<DuplexStreamingServerCallHandler<TService, TRequest, TResponse>.DuplexStreamingServerMethod>(callInfo.Channel);
            var handler = new DuplexStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }
    }
}
