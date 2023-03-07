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
using Grpc.AspNetCore.Server.Model;
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding;

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
        _rootConfiguration = GrpcPreconditions.CheckNotNull(rootConfiguration, nameof(rootConfiguration)).Value;
        _serviceConfiguration = GrpcPreconditions.CheckNotNull(serviceConfiguration, nameof(serviceConfiguration)).Value;
        _logger = GrpcPreconditions.CheckNotNull(logger, nameof(logger));
        _serviceProvider = GrpcPreconditions.CheckNotNull(serviceProvider, nameof(serviceProvider));
    }

    public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
    {
        var serviceType = typeof(TService);
        if (ServiceContract.IsNativeGrpcService(serviceType))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Ignore service {0} binding: native grpc service.", serviceType.FullName);
            }

            return;
        }

        var filterContext = new ServiceMethodFilterRegistration(_serviceProvider);
        filterContext.Add(_rootConfiguration.GetFilters());
        filterContext.Add(_serviceConfiguration.GetFilters());

        var marshallerFactory = (_serviceConfiguration.MarshallerFactory ?? _rootConfiguration.DefaultMarshallerFactory).ThisOrDefault();
        var serviceBinder = new AspNetCoreServiceMethodBinder<TService>(
            context,
            marshallerFactory,
            filterContext,
            _rootConfiguration.IsApiDescriptionRequested);

        CreateEndpointBinder().Bind(serviceBinder);
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

    private IServiceEndpointBinder<TService> CreateEndpointBinder()
    {
        if (_serviceConfiguration.EndpointBinderType == null)
        {
            return new EmitGenerator { Logger = new LogAdapter(_logger) }.GenerateServiceEndpointBinder<TService>(GetServiceInstanceType());
        }

        return (IServiceEndpointBinder<TService>)Activator.CreateInstance(_serviceConfiguration.EndpointBinderType)!;
    }
}