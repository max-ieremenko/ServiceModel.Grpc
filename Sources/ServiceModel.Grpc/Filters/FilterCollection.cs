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
using System.Collections.ObjectModel;

namespace ServiceModel.Grpc.Filters
{
    /// <summary>
    /// Represents the pipeline of filters to be invoked when processing a gRPC call.
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    public sealed class FilterCollection<TFilter> : Collection<FilterRegistration<TFilter>>
        where TFilter : class
    {
        /// <summary>
        /// Add a filter to the pipeline.
        /// </summary>
        /// <param name="order">The order value for determining the order of execution of filters.</param>
        /// <param name="filter">The filter instance.</param>
        /// <returns>Self.</returns>
        public FilterCollection<TFilter> Add(int order, TFilter filter)
        {
            filter.AssertNotNull(nameof(filter));

            Add(new FilterRegistration<TFilter>(order, _ => filter));
            return this;
        }

        /// <summary>
        /// Add a filter to the pipeline.
        /// </summary>
        /// <param name="order">The order value for determining the order of execution of filters.</param>
        /// <param name="factory">The filter factory.</param>
        /// <returns>Self.</returns>
        public FilterCollection<TFilter> Add(int order, Func<IServiceProvider, TFilter> factory)
        {
            Add(new FilterRegistration<TFilter>(order, factory));
            return this;
        }
    }
}
