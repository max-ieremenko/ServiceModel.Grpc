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

namespace ServiceModel.Grpc.Filters;

/// <summary>
/// A server filter registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public abstract class ServerFilterRegistrationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerFilterRegistrationAttribute"/> class.
    /// </summary>
    /// <param name="order">The order value for determining the order of execution of filters.</param>
    protected ServerFilterRegistrationAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets or sets the order value for determining the order of execution of filters.
    /// </summary>
    public int Order { get; protected set; }

    /// <summary>
    /// Create the filter instance.
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
    /// <returns>The filter instance.</returns>
    public abstract IServerFilter CreateFilter(IServiceProvider serviceProvider);
}