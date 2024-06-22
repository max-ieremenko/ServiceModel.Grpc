// <copyright>
// Copyright 2022 Max Ieremenko
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
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.SelfHost.Internal;

//// ReSharper disable CheckNamespace
namespace Grpc.Core;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a set of methods to simplify ServiceModel.Grpc services registration.
/// </summary>
public static class ServiceModelGrpcServiceBinderExtensions
{
    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="ServiceBinderBase"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="serviceBinder">The <see cref="ServiceBinderBase"/>.</param>
    /// <param name="serviceFactory">Method which creates a service instance.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="ServiceBinderBase"/>.</returns>
    public static ServiceBinderBase BindServiceModel<TService>(
        this ServiceBinderBase serviceBinder,
        Func<TService> serviceFactory,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(serviceBinder, nameof(serviceBinder));
        GrpcPreconditions.CheckNotNull(serviceFactory, nameof(serviceFactory));

        BindService(serviceBinder, null, serviceFactory, null, configure);
        return serviceBinder;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="ServiceBinderBase"/>.
    /// This method used by generated source code.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="serviceBinder">The <see cref="ServiceBinderBase"/>.</param>
    /// <param name="endpointBinder">The generated service endpoint binder.</param>
    /// <param name="serviceFactory">Method which creates a service instance.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="ServiceBinderBase"/>.</returns>
    public static ServiceBinderBase BindServiceModel<TService>(
        this ServiceBinderBase serviceBinder,
        IServiceEndpointBinder<TService> endpointBinder,
        Func<TService> serviceFactory,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(serviceBinder, nameof(serviceBinder));
        GrpcPreconditions.CheckNotNull(serviceFactory, nameof(serviceFactory));
        GrpcPreconditions.CheckNotNull(endpointBinder, nameof(endpointBinder));

        BindService(serviceBinder, endpointBinder, serviceFactory, null, configure);
        return serviceBinder;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="ServiceBinderBase"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="serviceBinder">The <see cref="ServiceBinderBase"/>.</param>
    /// <param name="serviceProvider">See <see cref="IServiceProvider"/>.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="ServiceBinderBase"/>.</returns>
    public static ServiceBinderBase BindServiceModel<TService>(
        this ServiceBinderBase serviceBinder,
        IServiceProvider serviceProvider,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(serviceBinder, nameof(serviceBinder));
        GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

        Func<TService> serviceFactory = serviceProvider.GetServiceRequired<TService>;
        var options = new ServiceModelGrpcServiceOptions
        {
            ServiceProvider = serviceProvider
        };

        BindService(serviceBinder, null, serviceFactory, options, configure);
        return serviceBinder;
    }

    /// <summary>
    /// Registers a ServiceModel.Grpc service in the <see cref="ServiceBinderBase"/>.
    /// This method used by generated source code.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="serviceBinder">The <see cref="ServiceBinderBase"/>.</param>
    /// <param name="endpointBinder">The generated service endpoint binder.</param>
    /// <param name="serviceProvider">See <see cref="IServiceProvider"/>.</param>
    /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
    /// <returns><see cref="ServiceBinderBase"/>.</returns>
    public static ServiceBinderBase BindServiceModel<TService>(
        this ServiceBinderBase serviceBinder,
        IServiceEndpointBinder<TService> endpointBinder,
        IServiceProvider serviceProvider,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(serviceBinder, nameof(serviceBinder));
        GrpcPreconditions.CheckNotNull(endpointBinder, nameof(endpointBinder));
        GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));

        Func<TService> serviceFactory = serviceProvider.GetServiceRequired<TService>;
        var options = new ServiceModelGrpcServiceOptions
        {
            ServiceProvider = serviceProvider
        };

        BindService(serviceBinder, endpointBinder, serviceFactory, options, configure);
        return serviceBinder;
    }

    private static void BindService<TService>(
        ServiceBinderBase serviceBinder,
        IServiceEndpointBinder<TService>? endpointBinder,
        Func<TService> serviceFactory,
        ServiceModelGrpcServiceOptions? options,
        Action<ServiceModelGrpcServiceOptions>? configure)
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

        // internal void BindService(ServiceBinderBase serviceBinder)
        var bindService = definition
            .GetType()
            .InstanceMethod("BindService", typeof(ServiceBinderBase))
            .CreateDelegate<Action<ServerServiceDefinition, ServiceBinderBase>>();

        bindService(definition, serviceBinder);
    }
}