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
/// A server filter that surrounds execution of the method.
/// </summary>
public interface IServerFilter
{
    /// <summary>
    /// Call handling method.
    /// </summary>
    /// <param name="context">The <see cref="IServerFilterContext"/> for the current call.</param>
    /// <param name="next">The delegate representing the remaining call in the request pipeline.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the execution of this filter.</returns>
    ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next);
}