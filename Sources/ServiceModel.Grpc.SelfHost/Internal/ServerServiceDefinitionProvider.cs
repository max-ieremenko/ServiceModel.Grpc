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
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class ServerServiceDefinitionProvider<TService>
    {
        private readonly Func<TService> _serviceFactory;
        private readonly IServiceEndpointBinder<TService>? _endpointBinder;
        private readonly ServiceModelGrpcServiceOptions? _options;

        public ServerServiceDefinitionProvider(
            Func<TService> serviceFactory,
            IServiceEndpointBinder<TService>? endpointBinder,
            ServiceModelGrpcServiceOptions? options)
        {
            _serviceFactory = serviceFactory;
            _endpointBinder = endpointBinder;
            _options = options;
        }

        public ServerServiceDefinition CreateDefinition()
        {
            var result = CreateServiceDefinition();

            if (_options?.ConfigureServiceDefinition != null)
            {
                result = _options.ConfigureServiceDefinition(result);
            }

            if (_options?.ErrorHandler != null)
            {
                result = result.Intercept(new ServerNativeInterceptor(new ServerCallErrorInterceptor(
                    _options.ErrorHandler,
                    _options.MarshallerFactory.ThisOrDefault())));
            }

            return result;
        }

        private ServerServiceDefinition CreateServiceDefinition()
        {
            var definitionBuilder = ServerServiceDefinition.CreateBuilder();
            var endpointBinder = GetOrCreateEndpointBinder();

            // SelfHostBinder must check ServiceProvider availability
            var filterRegistration = new ServiceMethodFilterRegistration(_options?.ServiceProvider!);
            filterRegistration.Add(_options?.GetFilters());

            var binder = new SelfHostServiceMethodBinder<TService>(
                (_options?.MarshallerFactory).ThisOrDefault(),
                WithLoggerFactory<TService>.Wrap(_serviceFactory, _options?.Logger),
                filterRegistration,
                definitionBuilder);
            endpointBinder.Bind(binder);

            return definitionBuilder.Build();
        }

        private IServiceEndpointBinder<TService> GetOrCreateEndpointBinder()
        {
            if (_endpointBinder != null)
            {
                return _endpointBinder;
            }

            var logger = new LogAdapter(_options?.Logger);

            var serviceInstanceType = typeof(TService);
            if (!ServiceContract.IsServiceInstanceType(serviceInstanceType))
            {
                serviceInstanceType = null;
            }

            return new EmitGenerator { Logger = logger }.GenerateServiceEndpointBinder<TService>(serviceInstanceType);
        }
    }
}
