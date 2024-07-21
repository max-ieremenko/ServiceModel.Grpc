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

using Grpc.Core.Utils;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSwag.Generation.Processors;
using ServiceModel.Grpc.AspNetCore.Internal.Swagger;
using ServiceModel.Grpc.AspNetCore.NSwag.Configuration;
using ServiceModel.Grpc.AspNetCore.NSwag.Internal;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a set of methods to simplify registration of ServiceModel.Grpc integration with NSwag.
/// </summary>
public static class ServiceModelSwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Enables integration of ServiceModel.Grpc with NSwag.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">The optional configuration action.</param>
    /// <returns>The the same <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceModelGrpcSwagger(
        this IServiceCollection services,
        Action<ServiceModelGrpcSwaggerOptions>? configure = default)
    {
        GrpcPreconditions.CheckNotNull(services, nameof(services));

        if (configure != null)
        {
            services.Configure(configure);
        }

        ServiceCollectionExtensions.AddSwagger(services, ResolveDataSerializer);
        services.TryAddEnumerable(ServiceDescriptor.Transient<IOperationProcessor, SwaggerOperationProcessor>());
        return services;
    }

    private static IDataSerializer ResolveDataSerializer(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<ServiceModelGrpcSwaggerOptions>>().Value;
        return new DataSerializer(options.JsonSerializer);
    }
}