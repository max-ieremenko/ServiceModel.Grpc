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
/// A client filter that surrounds execution of the method.
/// </summary>
public interface IClientFilter
{
    /// <summary>
    /// Blocking unary call handling method.
    /// </summary>
    /// <param name="context">The <see cref="IClientFilterContext"/> for the current call.</param>
    /// <param name="next">The delegate representing the remaining call in the request pipeline.</param>
    void Invoke(IClientFilterContext context, Action next);

    /// <summary>
    /// Async call handling method.
    /// </summary>
    /// <param name="context">The <see cref="IClientFilterContext"/> for the current call.</param>
    /// <param name="next">The delegate representing the remaining call in the request pipeline.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the execution of this filter.</returns>
    ValueTask InvokeAsync(IClientFilterContext context, Func<ValueTask> next);
}