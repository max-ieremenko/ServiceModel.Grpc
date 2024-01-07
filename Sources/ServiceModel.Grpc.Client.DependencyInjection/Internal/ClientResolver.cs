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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientResolver<TContract>
    where TContract : class
{
    private readonly string _optionsKey;
    private Func<IServiceProvider, object?, CallInvoker> _getCallInvoker;

    public ClientResolver(string optionsKey)
    {
        _optionsKey = optionsKey;
        _getCallInvoker = InitializeCallInvoker;
    }

    public static void Register(IServiceCollection services, object? serviceKey, string optionsKey)
    {
        services.TryAddKeyedTransient(serviceKey, new ClientResolver<TContract>(optionsKey).ResolveKeyed);
    }

    public static void Register(IHttpClientBuilder builder) => builder.ConfigureGrpcClientCreator(Resolve);

    private static TContract Resolve(IServiceProvider provider, CallInvoker invoker)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));
        GrpcPreconditions.CheckNotNull(invoker, nameof(invoker));

        var factory = provider.GetRequiredService<IClientFactory>();
        return factory.CreateClient<TContract>(invoker);
    }

    private TContract ResolveKeyed(IServiceProvider provider, object? key)
    {
        GrpcPreconditions.CheckNotNull(provider, nameof(provider));

        var factory = provider.Resolve<IClientFactory>(key);
        var invoker = _getCallInvoker(provider, key);

        return factory.CreateClient<TContract>(invoker);
    }

    private CallInvoker InitializeCallInvoker(IServiceProvider provider, object? key)
    {
        var options = KeyedServiceExtensions.ResolveOptions<ClientFactoryOptions>(provider, _optionsKey);

        var channelProvider = options.FindClient<TContract>()?.Channel ?? options.Channel;
        if (channelProvider == null)
        {
            channelProvider = ChannelProviderFactory.Default();
        }

        _getCallInvoker = channelProvider.GetCallInvoker;
        return _getCallInvoker(provider, key);
    }
}