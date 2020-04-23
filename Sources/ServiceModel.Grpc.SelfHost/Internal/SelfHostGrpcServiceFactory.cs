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

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class SelfHostGrpcServiceFactory<TService> : GrpcServiceFactoryBase<TService>
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ServerServiceDefinition.Builder _builder;

        public SelfHostGrpcServiceFactory(
            ILogger logger,
            IMarshallerFactory marshallerFactory,
            Func<TService> serviceFactory,
            ServerServiceDefinition.Builder builder)
            : base(logger, marshallerFactory)
        {
            _serviceFactory = serviceFactory;
            _builder = builder;
        }

        protected override void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<UnaryServerCallHandler<TService, TRequest, TResponse>.UnaryServerMethod>();
            var handler = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<ClientStreamingServerCallHandler<TService, TRequest, TResponse>.ClientStreamingServerMethod>();
            var handler = new ClientStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<ServerStreamingServerCallHandler<TService, TRequest, TResponse>.ServerStreamingServerMethod>();
            var handler = new ServerStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var invoker = callInfo.ChannelMethod.CreateDelegate<DuplexStreamingServerCallHandler<TService, TRequest, TResponse>.DuplexStreamingServerMethod>();
            var handler = new DuplexStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }
    }
}
