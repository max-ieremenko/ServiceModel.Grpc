// <copyright>
// Copyright Max Ieremenko
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

using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;

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
            var serviceInstanceType = typeof(TService);
            HostRegistration.BindWithEmit(binder, ServiceContract.IsServiceInstanceType(serviceInstanceType) ? serviceInstanceType : null, loggerAdapter);
        }
        else
        {
            endpointBinder.Bind(binder);
        }

        var definition = definitionBuilder.Build();

        if (options?.ConfigureServiceDefinition != null)
        {
            definition = options.ConfigureServiceDefinition(definition);
        }

        if (options?.ErrorHandler != null)
        {
            var errorInterceptor = ErrorHandlerInterceptorFactory.CreateServerHandler(
                options.ErrorHandler,
                options.MarshallerFactory.ThisOrDefault(),
                options.ErrorDetailSerializer,
                loggerAdapter);
            definition = definition.Intercept(errorInterceptor);
        }

        return definition;
    }

    private static void ValidateServiceType(Type serviceType)
    {
        if (ServiceContract.IsNativeGrpcService(serviceType))
        {
            throw new NotSupportedException($"{serviceType.FullName} is native grpc service.");
        }
    }
}