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
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Filters;

/// <summary>
/// Base attribute for inline server filter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public abstract class ServerFilterAttribute : Attribute, IServerFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerFilterAttribute"/> class.
    /// </summary>
    /// <param name="order">The order value for determining the order of execution of filters.</param>
    protected ServerFilterAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets or sets the order value for determining the order of execution of filters.
    /// </summary>
    public int Order { get; protected set; }

    /// <inheritdoc />
    public abstract ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next);
}