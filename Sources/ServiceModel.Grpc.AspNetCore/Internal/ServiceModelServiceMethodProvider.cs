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
using Grpc.AspNetCore.Server.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class ServiceModelServiceMethodProvider<TService> : IServiceMethodProvider<TService>
        where TService : class
    {
        private readonly ServiceModelGrpcServiceOptions _rootConfiguration;
        private readonly ServiceModelGrpcServiceOptions<TService> _serviceConfiguration;
        private readonly ILogger<ServiceModelServiceMethodProvider<TService>> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ServiceModelServiceMethodProvider(
            IOptions<ServiceModelGrpcServiceOptions> rootConfiguration,
            IOptions<ServiceModelGrpcServiceOptions<TService>> serviceConfiguration,
            ILogger<ServiceModelServiceMethodProvider<TService>> logger,
            IServiceProvider serviceProvider)
        {
            rootConfiguration.AssertNotNull(nameof(rootConfiguration));
            serviceConfiguration.AssertNotNull(nameof(serviceConfiguration));
            logger.AssertNotNull(nameof(logger));
            serviceProvider.AssertNotNull(nameof(serviceProvider));

            _rootConfiguration = rootConfiguration.Value;
            _serviceConfiguration = serviceConfiguration.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var serviceInstanceType = GetServiceInstanceType();

            var marshallerFactory = _serviceConfiguration.MarshallerFactory ?? _rootConfiguration.DefaultMarshallerFactory;
            var log = new LogAdapter(_logger);

            var factory = new AspNetCoreGrpcServiceFactory<TService>(log, context, serviceInstanceType, marshallerFactory);
            factory.Bind();
        }

        internal Type GetServiceInstanceType()
        {
            var serviceInstanceType = typeof(TService);
            if (ServiceContract.IsServiceInstanceType(serviceInstanceType))
            {
                return serviceInstanceType;
            }

            try
            {
                return _serviceProvider.GetRequiredService<TService>().GetType();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "A gRPC service binding is registered via {0}. Failed to resolve the implementation: {1}.".FormatWith(serviceInstanceType.GetShortAssemblyQualifiedName(), ex.Message),
                    ex);
            }
        }
    }
}
