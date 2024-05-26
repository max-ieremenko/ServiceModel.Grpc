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
using System.Globalization;
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Filters;

/// <summary>
/// Representation of a registration of the filter in the pipeline.
/// </summary>
/// <typeparam name="TFilter">The filter type.</typeparam>
public sealed class FilterRegistration<TFilter> : IComparable<FilterRegistration<TFilter>>
    where TFilter : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterRegistration{TFilter}"/> class.
    /// </summary>
    /// <param name="order">The order value for determining the order of execution of filters.</param>
    /// <param name="factory">The filter factory.</param>
    public FilterRegistration(int order, Func<IServiceProvider, TFilter> factory)
    {
        Order = order;
        Factory = GrpcPreconditions.CheckNotNull(factory, nameof(factory));
    }

    /// <summary>
    /// Gets the order value for determining the order of execution of filters.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the filter factory.
    /// </summary>
    public Func<IServiceProvider, TFilter> Factory { get; }

    /// <inheritdoc />
    public int CompareTo(FilterRegistration<TFilter>? other)
    {
        if (other == null)
        {
            return 1;
        }

        return Order.CompareTo(other.Order);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Order}: {typeof(TFilter).Name}";
}