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

using System;
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientFactoryBuilder : IClientFactoryBuilder
{
    private readonly ClientFactoryResolver _resolver;

    public ClientFactoryBuilder(IServiceCollection services, ClientFactoryResolver resolver)
    {
        _resolver = resolver;
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IClientFactoryBuilder ConfigureDefaultChannel(IChannelProvider channel)
    {
        GrpcPreconditions.CheckNotNull(channel, nameof(channel));

        _resolver.Channel = channel;

        return this;
    }

    public IClientFactoryBuilder AddClient<TContract>(
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null,
        IChannelProvider? channel = null)
        where TContract : class
    {
        ClientFactory.VerifyClient<TContract>();

        var client = FindClientResolver<TContract>();
        AddClientCore(client, null, configure, channel);

        return this;
    }

    public IClientFactoryBuilder AddClientBuilder<TContract>(
        IClientBuilder<TContract> builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class
    {
        var client = FindClientResolver<TContract>();
        AddClientCore(client, builder, configure, channel);

        return this;
    }

    internal void AddGrpcClient<TContract>(
        IClientBuilder<TContract>? builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure)
        where TContract : class
    {
        var client = FindClientResolver<TContract>();
        if (client?.Channel != null)
        {
            throw new NotSupportedException($"{typeof(TContract)} client is already registered with custom channel and cannot be used with Grpc.Net.ClientFactory.");
        }

        AddClientCore(client, builder, configure, null);
    }

    private void AddClientCore<TContract>(
        ClientResolver<TContract>? client,
        IClientBuilder<TContract>? builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class
    {
        var descriptor = FindClientDescriptor(typeof(TContract));
        if (descriptor != null)
        {
            if (configure == null && channel == null && builder == null)
            {
                return;
            }

            if (channel != null && descriptor.ImplementationFactory?.Target is not ClientResolver<TContract>)
            {
                throw new NotSupportedException($"{typeof(TContract)} client is already registered with Grpc.Net.ClientFactory and cannot be used with custom channel.");
            }
        }

        if (client == null)
        {
            client = new ClientResolver<TContract>();
            _resolver.Clients.Add(client);
        }

        if (configure != null)
        {
            client.Configure += configure;
        }

        if (channel != null)
        {
            client.Channel = channel;
        }

        if (builder != null)
        {
            client.Builder = builder;
        }

        if (descriptor == null)
        {
            Services.AddTransient(client.Resolve);
        }
    }

    private ClientResolver<TContract>? FindClientResolver<TContract>()
        where TContract : class
    {
        for (var i = 0; i < _resolver.Clients.Count; i++)
        {
            if (_resolver.Clients[i] is ClientResolver<TContract> client)
            {
                return client;
            }
        }

        return null;
    }

    private ServiceDescriptor? FindClientDescriptor(Type clientType)
    {
        for (var i = Services.Count - 1; i >= 0; i--)
        {
            var descriptor = Services[i];

            /*!descriptor.IsKeyedService && */
            if (descriptor.ServiceType == clientType)
            {
                return descriptor;
            }
        }

        return null;
    }
}