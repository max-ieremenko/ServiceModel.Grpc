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

using System.Collections.Generic;
using System.Reflection;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class AspNetCoreGrpcServiceFactory<TService> : IServiceBinder
        where TService : class
    {
        private readonly ILogger _logger;
        private readonly ServiceMethodProviderContext<TService> _context;
        private readonly IMarshallerFactory? _marshallerFactory;

        public AspNetCoreGrpcServiceFactory(
            ILogger logger,
            ServiceMethodProviderContext<TService> context,
            IMarshallerFactory? marshallerFactory)
        {
            _logger = logger;
            _context = context;
            _marshallerFactory = marshallerFactory;
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
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<UnaryServerMethod<TService, TRequest, TResponse>>(callInfo.Channel);
            _context.AddUnaryMethod(method, metadata, invoker);
        }

        public void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<ClientStreamingServerMethod<TService, TRequest, TResponse>>(callInfo.Channel);
            _context.AddClientStreamingMethod(method, metadata, invoker);
        }

        public void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<ServerStreamingServerMethod<TService, TRequest, TResponse>>(callInfo.Channel);
            _context.AddServerStreamingMethod(method, metadata, invoker);
        }

        public void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(callInfo.Channel);
            _context.AddDuplexStreamingMethod(method, metadata, invoker);
        }

        private static IList<object> GetMethodMetadata(MethodInfo serviceInstanceMethod)
        {
            // https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs
            var metadata = new List<object>();

            // Add type metadata first so it has a lower priority
            metadata.AddRange(typeof(TService).GetCustomAttributes(inherit: true));

            // Add method metadata last so it has a higher priority
            metadata.AddRange(serviceInstanceMethod.GetCustomAttributes(inherit: true));

            return metadata;
        }
    }
}
