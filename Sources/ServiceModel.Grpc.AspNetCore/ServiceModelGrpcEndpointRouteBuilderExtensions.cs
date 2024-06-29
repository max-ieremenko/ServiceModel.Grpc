// <copyright>
// Copyright 2020 Max Ieremenko
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

using Grpc.Core.Utils;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Internal;

//// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add ServiceModel.Grpc service endpoints.
/// </summary>
public static class ServiceModelGrpcEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps incoming requests to the specified <typeparamref name="TService"/> type.
    /// </summary>
    /// <typeparam name="TService">The service type to map requests to.</typeparam>
    /// <typeparam name="TEndpointBinder">The <see cref="IServiceEndpointBinder{TService}"/> to build endpoint.</typeparam>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <returns>A <see cref="GrpcServiceEndpointConventionBuilder"/> for endpoints associated with the service.</returns>
    public static GrpcServiceEndpointConventionBuilder MapGrpcService<TService, TEndpointBinder>(this IEndpointRouteBuilder builder)
        where TService : class
        where TEndpointBinder : IServiceEndpointBinder<TService>, new()
    {
        GrpcPreconditions.CheckNotNull(builder, nameof(builder));

        var options = builder.ServiceProvider.GetRequiredService<IOptions<ServiceModelGrpcServiceOptions<TService>>>();
        options.Value.EndpointBinderType = typeof(TEndpointBinder);

        return builder.MapGrpcService<TService>();
    }
}