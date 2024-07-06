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

using Grpc.Core.Logging;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Interceptors;

//// ReSharper disable CheckNamespace
namespace Grpc.Core;
//// ReSharper restore CheckNamespace

/// <summary>
/// Provides a configuration for a ServiceModel.Grpc services.
/// </summary>
public sealed class ServiceModelGrpcServiceOptions
{
    private FilterCollection<IServerFilter>? _filters;

    /// <summary>
    /// Gets or sets a factory for serializing and deserializing messages.
    /// </summary>
    public IMarshallerFactory? MarshallerFactory { get; set; }

    /// <summary>
    /// Gets or sets a server call error handler.
    /// </summary>
    public IServerErrorHandler? ErrorHandler { get; set; }

    /// <summary>
    /// Gets or sets logger to handle possible output from service binding process.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IServiceProvider"/>.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Gets or sets a method for additional <see cref="ServerServiceDefinition"/> configuration.
    /// </summary>
    public Func<ServerServiceDefinition, ServerServiceDefinition>? ConfigureServiceDefinition { get; set; }

    /// <summary>
    /// Gets the collection of registered server filters for this service.
    /// </summary>
    public FilterCollection<IServerFilter> Filters
    {
        get
        {
            if (_filters == null)
            {
                _filters = new FilterCollection<IServerFilter>();
            }

            return _filters;
        }
    }

    internal IList<FilterRegistration<IServerFilter>>? GetFilters() => _filters;
}