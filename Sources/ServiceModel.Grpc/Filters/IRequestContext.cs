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

using System.Collections.Generic;

namespace ServiceModel.Grpc.Filters
{
    /// <summary>
    /// Represents a request context for filters, specifically <see cref="IServerFilter.InvokeAsync"/>.
    /// </summary>
    public interface IRequestContext : IEnumerable<KeyValuePair<string, object?>>
    {
        /// <summary>
        /// Gets the number of arguments to pass when invoking the service method.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets or sets the client stream to pass when invoking the ClientStreaming or DuplexStreaming service method, <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        object? Stream { get; set; }

        /// <summary>
        /// Gets or sets the value of argument by name. The name comes from contract method definition <see cref="IServerFilterContext.ContractMethodInfo"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>The value of argument.</returns>
        object? this[string name] { get; set; }

        /// <summary>
        /// Gets or sets the value of argument by index. The index comes from contract method definition <see cref="IServerFilterContext.ContractMethodInfo"/>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns>The value of argument.</returns>
        object? this[int index] { get; set; }
    }
}
