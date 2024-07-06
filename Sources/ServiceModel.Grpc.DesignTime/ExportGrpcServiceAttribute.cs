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

namespace ServiceModel.Grpc.DesignTime;

/// <summary>
/// A marker to generate the source code for service hosting.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Conditional("CodeGeneration")]
public sealed class ExportGrpcServiceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportGrpcServiceAttribute"/> class.
    /// </summary>
    /// <param name="serviceType">The service type to map requests to.</param>
    public ExportGrpcServiceAttribute(Type serviceType)
    {
        ServiceType = GrpcPreconditions.CheckNotNull(serviceType, nameof(serviceType));
    }

    /// <summary>
    /// Gets the service type to map requests to.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate extension methods for AspNet hosting.
    /// </summary>
    public bool GenerateAspNetExtensions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate extension methods for Grpc.Core.Server hosting.
    /// </summary>
    public bool GenerateSelfHostExtensions { get; set; }
}