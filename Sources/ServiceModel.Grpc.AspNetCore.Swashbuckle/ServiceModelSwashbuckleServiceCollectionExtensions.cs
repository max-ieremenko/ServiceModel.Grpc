// <copyright>
// Copyright 2021 Max Ieremenko
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

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceModel.Grpc;
using ServiceModel.Grpc.AspNetCore.Swashbuckle.Internal;
using Swashbuckle.AspNetCore.SwaggerGen;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a set of methods to simplify ServiceModel.Grpc services registration.
    /// </summary>
    public static class ServiceModelSwashbuckleServiceCollectionExtensions
    {
        /// <summary>
        /// Enables integration of ServiceModel.Grpc with Swashbuckle.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The the same <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddServiceModelGrpcSwagger(this IServiceCollection services)
        {
            services.AssertNotNull(nameof(services));

            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, ApiDescriptionProvider>());
            services.ConfigureSwaggerGen(ConfigureSwagger);
            return services;
        }

        private static void ConfigureSwagger(SwaggerGenOptions options)
        {
            var filters = options.OperationFilterDescriptors;
            for (var i = 0; i < filters.Count; i++)
            {
                if (filters[i].Type == typeof(SwaggerOperationFilter))
                {
                    return;
                }
            }

            options.OperationFilter<SwaggerOperationFilter>();
        }
    }
}