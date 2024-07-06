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
using Grpc.Core;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client;

/// <summary>
/// Represents a type used to configure and create instances of gRPC service clients.
/// </summary>
public interface IClientFactory
{
    /// <summary>
    /// Configures the factory to generate a proxy automatically (Reflection.Emit) for gRPC service contract <typeparamref name="TContract"/> with specific options.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="configure">The action to configure options.</param>
    void AddClient<TContract>(Action<ServiceModelGrpcClientOptions>? configure = null)
        where TContract : class;

    /// <summary>
    /// Configures the factory to use a proxy builder for gRPC service contract <typeparamref name="TContract"/> with specific options.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="builder">The proxy builder.</param>
    /// <param name="configure">The action to configure options.</param>
    void AddClient<TContract>(IClientBuilder<TContract> builder, Action<ServiceModelGrpcClientOptions>? configure = null)
        where TContract : class;

    /// <summary>
    /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="channel">The gRPC channel.</param>
    /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
    TContract CreateClient<TContract>(ChannelBase channel)
        where TContract : class;

    /// <summary>
    /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="callInvoker">The client-side RPC invocation.</param>
    /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
    TContract CreateClient<TContract>(CallInvoker callInvoker)
        where TContract : class;
}