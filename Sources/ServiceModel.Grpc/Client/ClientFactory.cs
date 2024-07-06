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

using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client;

/// <summary>
/// Serves to configure and create instances of gRPC service clients.
/// </summary>
public sealed class ClientFactory : IClientFactory
{
    private readonly ServiceModelGrpcClientOptions? _defaultOptions;
    private readonly ConcurrentDictionary<Type, ClientRegistration> _registrationByContract;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientFactory"/> class.
    /// </summary>
    /// <param name="defaultOptions">Default configuration for all clients, created by this instance.</param>
    public ClientFactory(ServiceModelGrpcClientOptions? defaultOptions = null)
    {
        _defaultOptions = defaultOptions;
        _registrationByContract = new ConcurrentDictionary<Type, ClientRegistration>();
    }

    /// <summary>
    /// Determines whether the <typeparamref name="TContract"/> is valid for creating proxy instances.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    public static void VerifyClient<TContract>()
    {
        var contractType = typeof(TContract);
        if (!contractType.IsInterface || !(contractType.IsPublic || contractType.IsNestedPublic) || contractType.IsGenericTypeDefinition)
        {
            throw new NotSupportedException($"{contractType} is not supported. Client contract must be public interface.");
        }
    }

    /// <summary>
    /// Configures the factory to generate a proxy automatically for gRPC service contract <typeparamref name="TContract"/> with specific options.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="configure">The action to configure options.</param>
    public void AddClient<TContract>(Action<ServiceModelGrpcClientOptions>? configure = null)
        where TContract : class
    {
        RegisterClient<TContract>(null, configure, false);
    }

    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="builder">The proxy builder.</param>
    /// <param name="configure">The action to configure options.</param>
    public void AddClient<TContract>(IClientBuilder<TContract> builder, Action<ServiceModelGrpcClientOptions>? configure = null)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        RegisterClient(builder, configure, false);
    }

    /// <summary>
    /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="channel">The gRPC channel.</param>
    /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
    public TContract CreateClient<TContract>(ChannelBase channel)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(channel, nameof(channel));

        return CreateClient<TContract>(channel.CreateCallInvoker());
    }

    /// <summary>
    /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="callInvoker">The client-side RPC invocation.</param>
    /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
    public TContract CreateClient<TContract>(CallInvoker callInvoker)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(callInvoker, nameof(callInvoker));

        if (!_registrationByContract.TryGetValue(typeof(TContract), out var registration))
        {
            registration = RegisterClient<TContract>(null, null, true);
        }

        return registration.Create<TContract>(callInvoker);
    }

    private ClientRegistration RegisterClient<TContract>(IClientBuilder<TContract>? userBuilder, Action<ServiceModelGrpcClientOptions>? configure, bool ignoreDuplication)
        where TContract : class
    {
        if (userBuilder == null)
        {
            VerifyClient<TContract>();
        }

        var contractType = typeof(TContract);
        var duplication = true;
        if (!_registrationByContract.TryGetValue(contractType, out var registration))
        {
            registration = ClientRegistration.Build(userBuilder, _defaultOptions, configure);
            duplication = !_registrationByContract.TryAdd(contractType, registration);
        }

        if (duplication && !ignoreDuplication)
        {
            throw new InvalidOperationException($"Client for contract {contractType.FullName} is already initialized and cannot be changed.");
        }

        return registration;
    }
}