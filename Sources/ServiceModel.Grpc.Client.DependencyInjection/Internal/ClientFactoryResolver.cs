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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientFactoryResolver
{
    private const string LoggerName = "ServiceModel.Grpc.Client";
    private readonly string _optionsKey;

    public ClientFactoryResolver(string optionsKey)
    {
        _optionsKey = optionsKey;
    }

    public static void Register(IServiceCollection services, object? serviceKey, string optionsKey)
    {
        services.TryAddKeyedSingleton(serviceKey, new ClientFactoryResolver(optionsKey).Create);
    }

    internal IClientFactory Create(IServiceProvider provider, Func<ServiceModelGrpcClientOptions, IClientFactory>? test)
    {
        var clientOptions = KeyedServiceExtensions.ResolveOptions<ServiceModelGrpcClientOptions>(provider, _optionsKey);
        if (clientOptions.ServiceProvider == null)
        {
            clientOptions.ServiceProvider = provider;
        }

        if (clientOptions.Logger == null)
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(LoggerName);
            clientOptions.Logger = LogAdapter.Wrap(logger);
        }

        var clientFactory = test == null ? new ClientFactory(clientOptions) : test(clientOptions);

        var factoryOptions = KeyedServiceExtensions.ResolveOptions<ClientFactoryOptions>(provider, _optionsKey);
        for (var i = 0; i < factoryOptions.Clients.Count; i++)
        {
            var client = factoryOptions.Clients[i];
            client.Setup(provider, clientFactory);
        }

        return clientFactory;
    }

    private IClientFactory Create(IServiceProvider provider, object? key) => Create(provider, null);
}