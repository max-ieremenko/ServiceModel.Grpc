// <copyright>
// Copyright 2024 Max Ieremenko
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
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientOptions<TContract> : IClientOptions
    where TContract : class
{
    private bool _suppressCustomChannel;
    private Action<ServiceModelGrpcClientOptions, IServiceProvider>? _configure;
    private IClientBuilder<TContract>? _builder;

    public IChannelProvider? Channel { get; private set; }

    public void Combine(
        IClientBuilder<TContract>? builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
    {
        if (builder != null)
        {
            _builder = builder;
        }

        _configure += configure;

        if (channel != null)
        {
            Channel = channel;
        }
    }

    public void SuppressCustomChannel() => _suppressCustomChannel = true;

    public void Setup(IServiceProvider provider, IClientFactory clientFactory)
    {
        if (_suppressCustomChannel && Channel != null)
        {
            throw new NotSupportedException($"{typeof(TContract)} client is registered with custom channel and is used by Grpc.Net.ClientFactory.");
        }

        Action<ServiceModelGrpcClientOptions>? configure = null;
        if (_configure != null)
        {
            configure = options => _configure(options, provider);
        }

        if (_builder == null)
        {
            clientFactory.AddClient<TContract>(configure);
        }
        else
        {
            clientFactory.AddClient(_builder, configure);
        }
    }
}