// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using Grpc.Core.Utils;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.SelfHost.Internal;

//// ReSharper disable CheckNamespace
namespace Grpc.Core;
//// ReSharper restore CheckNamespace

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
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModelTransient<TService>(
        this Server.ServiceDefinitionCollection services,
        Func<TService> serviceFactory,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(serviceFactory, nameof(serviceFactory));

        BindService(services, serviceFactory, null, configure);
        return services;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service (one instance per-call) in the <see cref="Server.ServiceDefinitionCollection"/>.
    /// This method used by generated source code.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
    /// <param name="serviceFactory">Method which creates a service instance.</param>
    /// <param name="endpointBinder">The generated service endpoint binder.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModelTransient<TService>(
        this Server.ServiceDefinitionCollection services,
        Func<TService> serviceFactory,
        IServiceEndpointBinder<TService> endpointBinder,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(serviceFactory, nameof(serviceFactory));
        GrpcPreconditions.CheckNotNull(endpointBinder, nameof(endpointBinder));

        BindService(services, serviceFactory, endpointBinder, configure);
        return services;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service (one instance for all calls) in the <see cref="Server.ServiceDefinitionCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
    /// <param name="service">The service instance.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModelSingleton<TService>(
        this Server.ServiceDefinitionCollection services,
        TService service,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(service, nameof(service));

        BindService(services, () => service, null, configure);
        return services;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service (one instance for all calls) in the <see cref="Server.ServiceDefinitionCollection"/>.
    /// This method used by generated source code.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
    /// <param name="service">The service instance.</param>
    /// <param name="endpointBinder">The generated service endpoint binder.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModelSingleton<TService>(
        this Server.ServiceDefinitionCollection services,
        TService service,
        IServiceEndpointBinder<TService> endpointBinder,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(service, nameof(service));
        GrpcPreconditions.CheckNotNull(endpointBinder, nameof(endpointBinder));

        BindService(services, () => service, endpointBinder, configure);
        return services;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="Server.ServiceDefinitionCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
    /// <param name="serviceProvider">See <see cref="IServiceProvider"/>.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModel<TService>(
        this Server.ServiceDefinitionCollection services,
        IServiceProvider serviceProvider,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

        Func<TService> serviceFactory = serviceProvider.GetServiceRequired<TService>;
        var options = new ServiceModelGrpcServiceOptions
        {
            ServiceProvider = serviceProvider
        };

        BindService(services, serviceFactory, null, configure, options);
        return services;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="Server.ServiceDefinitionCollection"/>.
    /// This method used by generated source code.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
    /// <param name="serviceProvider">See <see cref="IServiceProvider"/>.</param>
    /// <param name="endpointBinder">The generated service endpoint binder.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="Server.ServiceDefinitionCollection"/>.</returns>
    public static Server.ServiceDefinitionCollection AddServiceModel<TService>(
        this Server.ServiceDefinitionCollection services,
        IServiceProvider serviceProvider,
        IServiceEndpointBinder<TService> endpointBinder,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));
        GrpcPreconditions.CheckNotNull(endpointBinder, nameof(endpointBinder));

        Func<TService> serviceFactory = serviceProvider.GetServiceRequired<TService>;
        var options = new ServiceModelGrpcServiceOptions
        {
            ServiceProvider = serviceProvider
        };

        BindService(services, serviceFactory, null, configure, options);
        return services;
    }

    private static void BindService<TService>(
        Server.ServiceDefinitionCollection services,
        Func<TService> serviceFactory,
        IServiceEndpointBinder<TService>? endpointBinder,
        Action<ServiceModelGrpcServiceOptions>? configure,
        ServiceModelGrpcServiceOptions? options = null)
    {
        if (configure != null)
        {
            if (options == null)
            {
                options = new ServiceModelGrpcServiceOptions();
            }

            configure(options);
        }

        var definition = ServiceDefinitionFactory.CreateDefinition(serviceFactory, endpointBinder, options);
        services.Add(definition);
    }
}