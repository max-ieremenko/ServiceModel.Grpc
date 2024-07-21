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

using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.Interceptors;

namespace ServiceModel.Grpc.Client;

/// <summary>
/// Provides configuration used by <see cref="IClientFactory"/>.
/// </summary>
public sealed class ServiceModelGrpcClientOptions
{
    private FilterCollection<IClientFilter>? _filters;

    /// <summary>
    /// Gets or sets a factory for serializing and deserializing messages.
    /// </summary>
    public IMarshallerFactory? MarshallerFactory { get; set; }

    /// <summary>
    /// Gets or sets a methods which provides <see cref="CallOptions"/> for all calls made by all clients created by <see cref="IClientFactory"/>.
    /// </summary>
    public Func<CallOptions>? DefaultCallOptionsFactory { get; set; }

    /// <summary>
    /// Gets or sets a client call error handler.
    /// </summary>
    public IClientErrorHandler? ErrorHandler { get; set; }

    /// <summary>
    /// Gets or sets an error details deserializer, that overrides default deserialization.
    /// It is only applicable with <see cref="ErrorHandler"/>.
    /// </summary>
    public IClientFaultDetailDeserializer? ErrorDetailDeserializer { get; set; }

    /// <summary>
    /// Gets or sets logger to handle possible output from <see cref="IClientFactory"/>.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IServiceProvider"/>.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Gets the collection of registered client filters.
    /// </summary>
    public FilterCollection<IClientFilter> Filters
    {
        get
        {
            if (_filters == null)
            {
                _filters = new FilterCollection<IClientFilter>();
            }

            return _filters;
        }
    }

    internal IList<FilterRegistration<IClientFilter>>? GetFilters() => _filters;
}