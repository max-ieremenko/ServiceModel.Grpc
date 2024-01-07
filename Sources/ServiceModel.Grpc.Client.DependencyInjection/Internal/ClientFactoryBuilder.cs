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
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientFactoryBuilder : IClientFactoryBuilder
{
    public ClientFactoryBuilder(IServiceCollection services, object? serviceKey, string optionsKey)
    {
        Services = services;
        ServiceKey = serviceKey;
        OptionsKey = optionsKey;
    }

    public IServiceCollection Services { get; }

    public object? ServiceKey { get; }

    public string OptionsKey { get; }

    public IClientFactoryBuilder ConfigureDefaultChannel(IChannelProvider channel)
    {
        GrpcPreconditions.CheckNotNull(channel, nameof(channel));

        ConfigureClientFactory(options =>
        {
            options.Channel = channel;
        });

        return this;
    }

    public IClientFactoryBuilder AddClient<TContract>(
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null,
        IChannelProvider? channel = null)
        where TContract : class
    {
        ClientFactory.VerifyClient<TContract>();

        if (configure != null || channel != null)
        {
            ConfigureClientFactory(options =>
            {
                options.FindOrCreateClient<TContract>().Combine(null, configure, channel);
            });
        }

        ClientResolver<TContract>.Register(Services, ServiceKey, OptionsKey);
        return this;
    }

    public IClientFactoryBuilder AddClientBuilder<TContract>(
        IClientBuilder<TContract> builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        ConfigureClientFactory(options =>
        {
            options.FindOrCreateClient<TContract>().Combine(builder, configure, channel);
        });

        ClientResolver<TContract>.Register(Services, ServiceKey, OptionsKey);
        return this;
    }

    internal void AddGrpcClient<TContract>(
        IClientBuilder<TContract>? builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure)
        where TContract : class
    {
        ConfigureClientFactory(options =>
        {
            var client = options.FindOrCreateClient<TContract>();
            client.Combine(builder, configure, null);
            client.SuppressCustomChannel();
        });
    }

    private void ConfigureClientFactory(Action<ClientFactoryOptions> configureOptions)
    {
        Services.AddOptions<ClientFactoryOptions>(OptionsKey).Configure(configureOptions);
    }
}