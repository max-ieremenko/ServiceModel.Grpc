// <copyright>
// Copyright 2024 Max Ieremenko
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

namespace ServiceModel.Grpc.Channel;

/// <summary>
/// Represents a type which is the unit of communication between endpoints.
/// </summary>
/// <typeparam name="T1">The value type.</typeparam>
public interface IMessage<T1>
{
    /// <summary>
    /// Gets the current value.
    /// </summary>
    /// <returns>The current value.</returns>
    T1? GetValue1();

    /// <summary>
    /// Sets the current value.
    /// </summary>
    /// <param name="value">The new value.</param>
    void SetValue1(T1? value);
}