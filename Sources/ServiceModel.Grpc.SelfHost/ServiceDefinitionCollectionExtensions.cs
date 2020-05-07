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
using Grpc.Core.Interceptors;
using ServiceModel.Grpc;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.SelfHost.Internal;

//// ReSharper disable CheckNamespace
namespace Grpc.Core
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a set of methods to simplify ServiceModel.Grpc services registration.
    /// </summary>
    public static class ServiceDefinitionCollectionExtensions
    {
        /// <summary>
        /// Registers a ServiceModel.Grpc service (one instance per-call) in the <see cref="Server.ServiceDefinitionCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
        /// <param name="serviceFactory">Method which creates a service instance.</param>
        /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
        public static void AddServiceModelTransient<TService>(
            this Server.ServiceDefinitionCollection services,
            Func<TService> serviceFactory,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            services.AssertNotNull(nameof(services));
            serviceFactory.AssertNotNull(nameof(serviceFactory));

            if (ServiceContract.IsNativeGrpcService(typeof(TService)))
            {
                throw new InvalidOperationException("{0} is native grpc service.".FormatWith(typeof(TService).FullName));
            }

            ServiceModelGrpcServiceOptions options = null;
            if (configure != null)
            {
                options = new ServiceModelGrpcServiceOptions();
                configure(options);
            }

            var builder = ServerServiceDefinition.CreateBuilder();
            var logger = new LogAdapter(options?.Logger);

            var factory = new SelfHostGrpcServiceFactory<TService>(
                logger,
                options?.MarshallerFactory,
                serviceFactory,
                builder);

            factory.Bind();

            var definition = builder.Build();
            if (options?.ConfigureServiceDefinition != null)
            {
                definition = options.ConfigureServiceDefinition(definition);
            }

            if (options?.ErrorHandler != null)
            {
                definition = definition.Intercept(new ServerNativeInterceptor(new ServerCallErrorInterceptor(
                    options.ErrorHandler,
                    options.MarshallerFactory.ThisOrDefault())));
            }

            services.Add(definition);
        }

        /// <summary>
        /// Registers a ServiceModel.Grpc service (one instance for all calls) in the <see cref="Server.ServiceDefinitionCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
        /// <param name="service">The service instance.</param>
        /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
        public static void AddServiceModelSingleton<TService>(
            this Server.ServiceDefinitionCollection services,
            TService service,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            AddServiceModelTransient(services, () => service, configure);
        }
    }
}
