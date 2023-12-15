// <copyright>
// Copyright 2023 Max Ieremenko
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
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection;

/// <summary>
/// A builder for configuring <see cref="ClientFactory"/> instance.
/// </summary>
public interface IClientFactoryBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Configures default <see cref="IChannelProvider"/> for the <see cref="ClientFactory"/>.
    /// </summary>
    /// <param name="callInvoker">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>Self.</returns>
    IClientFactoryBuilder ConfigureDefaultChannel(IChannelProvider callInvoker);

    /// <summary>
    /// Registers transient <typeparamref name="TContract"/> client and related services to the <see cref="IServiceCollection"/> and configures
    /// a binding between the <typeparamref name="TContract"/> type and the <see cref="ClientFactory"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>Self.</returns>
    IClientFactoryBuilder AddClient<TContract>(
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null,
        IChannelProvider? channel = null)
        where TContract : class;

    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="builder">The proxy builder.</param>
    /// <param name="configure">A delegate that is used to configure a client.</param>
    /// <param name="channel">An instance that can provide <see cref="CallInvoker"/> instance for gRPC client calls, see also <see cref="ChannelProviderFactory"/>.</param>
    /// <returns>Self.</returns>
    IClientFactoryBuilder AddClientBuilder<TContract>(
        IClientBuilder<TContract> builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure,
        IChannelProvider? channel)
        where TContract : class;
}