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
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;
using ServiceModel.Grpc.AspNetCore.Internal.Binding;
using ServiceModel.Grpc.AspNetCore.Internal.Swagger;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a set of methods to simplify ServiceModel.Grpc services registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enables ServiceModel.Grpc services for the specific <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">The optional configuration action to provide default configuration for all ServiceModel.Grpc services.</param>
    /// <returns>The <see cref="IGrpcServerBuilder"/>.</returns>
    public static IGrpcServerBuilder AddServiceModelGrpc(
        this IServiceCollection services,
        Action<ServiceModelGrpcServiceOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        if (configure != null)
        {
            services.Configure(configure);
            services.TryAddTransient<IPostConfigureOptions<GrpcServiceOptions>, PostConfigureGrpcServiceOptions>();
        }

        var builder = services.AddGrpc();
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IServiceMethodProvider<>), typeof(ServiceModelServiceMethodProvider<>)));
        return builder;
    }

    /// <summary>
    /// Registers a configuration for the specific ServiceModel.Grpc service in the <see cref="IGrpcServerBuilder"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="builder">The <see cref="IGrpcServerBuilder"/>.</param>
    /// <param name="configure">The configuration action to provide a configuration the specific ServiceModel.Grpc service.</param>
    /// <returns><see cref="IGrpcServerBuilder"/>.</returns>
    public static IGrpcServerBuilder AddServiceModelGrpcServiceOptions<TService>(
        this IGrpcServerBuilder builder,
        Action<ServiceModelGrpcServiceOptions<TService>> configure)
        where TService : class
    {
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        AddServiceModelGrpcServiceOptions(builder.Services, configure);
        return builder;
    }

    /// <summary>
    /// Registers a configuration for the specific ServiceModel.Grpc service in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">The configuration action to provide a configuration the specific ServiceModel.Grpc service.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceModelGrpcServiceOptions<TService>(
        this IServiceCollection services,
        Action<ServiceModelGrpcServiceOptions<TService>> configure)
        where TService : class
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(configure, nameof(configure));

        services.Configure(configure);
        services.TryAddTransient<IPostConfigureOptions<GrpcServiceOptions<TService>>, PostConfigureGrpcServiceOptions<TService>>();

        return services;
    }

    internal static void AddSwagger(IServiceCollection services, Func<IServiceProvider, IDataSerializer> serializerFactory)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, ApiDescriptionProvider>());
        services.AddTransient<IApiDescriptionAdapter, ApiDescriptionAdapter>();
        services.AddTransient<ISwaggerUiRequestHandler, SwaggerUiRequestHandler>();
        services.AddTransient(serializerFactory);
        services.Configure<ServiceModelGrpcServiceOptions>(RequestApiDescription);
    }

    private static void RequestApiDescription(ServiceModelGrpcServiceOptions options)
    {
        options.IsApiDescriptionRequested = true;
    }
}