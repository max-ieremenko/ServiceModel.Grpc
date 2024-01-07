// <copyright>
// Copyright 2023-2024 Max Ieremenko
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
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.DependencyInjection.Internal;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection;

/// <summary>
/// Provides a set of methods to simplify ServiceModel.Grpc clients registration.
/// </summary>
public static class ClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers singleton <see cref="ClientFactory"/> and related services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">A delegate that is used to configure all clients, created by this <see cref="ClientFactory"/>.</param>
    /// <returns>An <see cref="IClientFactoryBuilder"/> that can be used to configure the factory.</returns>
    public static IClientFactoryBuilder AddServiceModelGrpcClientFactory(
        this IServiceCollection services,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        return AddKeyedServiceModelGrpcClientFactory(services, serviceKey: null, configure);
    }

    /// <summary>
    /// Registers singleton <see cref="ClientFactory"/> and related services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="serviceKey">The service key.</param>
    /// <param name="configure">A delegate that is used to configure all clients, created by this <see cref="ClientFactory"/>.</param>
    /// <returns>An <see cref="IClientFactoryBuilder"/> that can be used to configure the factory.</returns>
    public static IClientFactoryBuilder AddKeyedServiceModelGrpcClientFactory(
        this IServiceCollection services,
        object? serviceKey,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        var optionsKey = KeyedServiceExtensions.GetOptionsKey(serviceKey);
        if (configure != null)
        {
            services.AddOptions<ServiceModelGrpcClientOptions>(optionsKey).Configure(configure);
        }

        ClientFactoryResolver.Register(services, serviceKey, optionsKey);
        return new ClientFactoryBuilder(services, serviceKey, optionsKey);
    }

    /// <summary>
    /// Registers transient <typeparamref name="TContract"/> client and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TContract"/> type and the <see cref="ClientFactory"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddServiceModelGrpcClient<TContract>(
        this IServiceCollection services,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null,
        IChannelProvider? channel = null)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        services
            .AddServiceModelGrpcClientFactory()
            .AddClient<TContract>(configure, channel);

        return services;
    }

    /// <summary>
    /// Registers transient <typeparamref name="TContract"/> client and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TContract"/> type and the <see cref="ClientFactory"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="serviceKey">The service key.</param>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddKeyedServiceModelGrpcClient<TContract>(
        this IServiceCollection services,
        object? serviceKey,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null,
        IChannelProvider? channel = null)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        services
            .AddKeyedServiceModelGrpcClientFactory(serviceKey)
            .AddClient<TContract>(configure, channel);

        return services;
    }

    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="builder">The proxy builder.</param>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddServiceModelGrpcClientBuilder<TContract>(
        IServiceCollection services,
        IClientBuilder<TContract> builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        services
            .AddServiceModelGrpcClientFactory()
            .AddClientBuilder(builder, configure, channel);

        return services;
    }

    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="serviceKey">The service key.</param>
    /// <param name="builder">The proxy builder.</param>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddKeyedServiceModelGrpcClientBuilder<TContract>(
        IServiceCollection services,
        object? serviceKey,
        IClientBuilder<TContract> builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        services
            .AddKeyedServiceModelGrpcClientFactory(serviceKey)
            .AddClientBuilder(builder, configure, channel);

        return services;
    }
}