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

using System;

//// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a set of methods to simplify registration of ServiceModel.Grpc integration with NSwag.
/// </summary>
public static class ServiceModelSwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// Enables HTTP/1.1 JSON Swagger UI gateway.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    /// <returns>The <paramref name="app"/>.</returns>
    public static IApplicationBuilder UseServiceModelGrpcSwaggerGateway(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        ServiceModel.Grpc.AspNetCore.ApplicationBuilderExtensions.UseSwagger(app);
        return app;
    }
}