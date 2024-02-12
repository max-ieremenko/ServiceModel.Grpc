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
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.SelfHost.Internal;

internal static class ServiceDefinitionFactory
{
    public static ServerServiceDefinition CreateDefinition<TService>(
        Func<TService> serviceFactory,
        IServiceEndpointBinder<TService>? endpointBinder,
        ServiceModelGrpcServiceOptions? options)
    {
        ValidateServiceType(typeof(TService));

        // SelfHostBinder must check ServiceProvider availability
        var filterRegistration = new ServiceMethodFilterRegistration(options?.ServiceProvider!);
        filterRegistration.Add(options?.Filters);

        var definitionBuilder = ServerServiceDefinition.CreateBuilder();

        var loggerAdapter = LogAdapter.Wrap(options?.Logger);

        var binder = new SelfHostServiceMethodBinder<TService>(
            (options?.MarshallerFactory).ThisOrDefault(),
            WithLoggerFactory<TService>.Wrap(serviceFactory, loggerAdapter),
            filterRegistration,
            definitionBuilder);

        if (endpointBinder == null)
        {
            endpointBinder = CreateDefaultEndpointBinder<TService>(loggerAdapter);
        }

        endpointBinder.Bind(binder);

        var definition = definitionBuilder.Build();

        if (options?.ConfigureServiceDefinition != null)
        {
            definition = options.ConfigureServiceDefinition(definition);
        }

        if (options?.ErrorHandler != null)
        {
            var errorInterceptor = new ServerCallErrorInterceptor(
                options.ErrorHandler,
                options.MarshallerFactory.ThisOrDefault(),
                loggerAdapter);
            definition = definition.Intercept(new ServerNativeInterceptor(errorInterceptor));
        }

        return definition;
    }

    private static void ValidateServiceType(Type serviceType)
    {
        if (ServiceContract.IsNativeGrpcService(serviceType))
        {
            throw new NotSupportedException("{0} is native grpc service.".FormatWith(serviceType.FullName));
        }
    }

    private static IServiceEndpointBinder<TService> CreateDefaultEndpointBinder<TService>(ILogger? logger)
    {
        var serviceInstanceType = typeof(TService);
        if (!ServiceContract.IsServiceInstanceType(serviceInstanceType))
        {
            serviceInstanceType = null;
        }

        return new EmitGenerator { Logger = logger }.GenerateServiceEndpointBinder<TService>(serviceInstanceType);
    }
}