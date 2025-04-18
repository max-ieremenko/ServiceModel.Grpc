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

using ServiceModel.Grpc.AspNetCore.Swashbuckle.Configuration;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a configuration for ServiceModel.Grpc integration with Swashbuckle.AspNetCore.
/// </summary>
public sealed class ServiceModelGrpcSwaggerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceModelGrpcSwaggerOptions"/> class with <see cref="SystemTextJsonSerializer"/>.
    /// </summary>
    public ServiceModelGrpcSwaggerOptions()
    {
        JsonSerializer = new SystemTextJsonSerializer();
        AutogenerateOperationSummaryAndDescription = true;
    }

    /// <summary>
    /// Gets or sets the <see cref="IJsonSerializer"/> which handles data serialization and deserialization for HTTP/1.1 JSON Swagger UI gateway.
    /// </summary>
    public IJsonSerializer JsonSerializer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to add method type into operation summary and method signature into description (default: true).
    /// </summary>
    public bool AutogenerateOperationSummaryAndDescription { get; set; }
}