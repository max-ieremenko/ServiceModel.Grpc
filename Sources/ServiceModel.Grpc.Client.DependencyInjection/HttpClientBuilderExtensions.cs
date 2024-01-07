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
using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.DependencyInjection.Internal;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Client.DependencyInjection;

/// <summary>
/// Extensions methods to configure an <see cref="IHttpClientBuilder"/>, provided by Grpc.Net.ClientFactory.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Configure Grpc.Net.ClientFactory to create gRPC client instances by ServiceModel.Grpc.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/>, provided by Grpc.Net.ClientFactory.AddGrpcClient.</param>
    /// <param name="configure">A delegate that is used to configure the gRPC client options.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder ConfigureServiceModelGrpcClientCreator<TContract>(
        this IHttpClientBuilder builder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure = null)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));
        ClientFactory.VerifyClient<TContract>();

        ClientResolver<TContract>.Register(builder);

        var factoryBuilder = (ClientFactoryBuilder)builder.Services.AddServiceModelGrpcClientFactory();
        factoryBuilder.AddGrpcClient<TContract>(null, configure);

        return builder;
    }

    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    /// <typeparam name="TContract">The service contract type.</typeparam>
    /// <param name="httpBuilder">The <see cref="IHttpClientBuilder"/>, provided by Grpc.Net.ClientFactory.AddGrpcClient.</param>
    /// <param name="serviceModelBuilder">The proxy builder.</param>
    /// <param name="configure">A delegate that is used to configure the gRPC client options.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder ConfigureServiceModelGrpcClientBuilder<TContract>(
        IHttpClientBuilder httpBuilder,
        IClientBuilder<TContract> serviceModelBuilder,
        Action<ServiceModelGrpcClientOptions, IServiceProvider>? configure)
        where TContract : class
    {
        GrpcPreconditions.CheckNotNull(httpBuilder, nameof(httpBuilder));
        GrpcPreconditions.CheckNotNull(serviceModelBuilder, nameof(serviceModelBuilder));

        ClientResolver<TContract>.Register(httpBuilder);

        var factoryBuilder = (ClientFactoryBuilder)httpBuilder.Services.AddServiceModelGrpcClientFactory();
        factoryBuilder.AddGrpcClient(serviceModelBuilder, configure);

        return httpBuilder;
    }
}