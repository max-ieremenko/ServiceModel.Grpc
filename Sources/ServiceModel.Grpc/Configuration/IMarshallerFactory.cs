// <copyright>
// Copyright 2020 Max Ieremenko
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

namespace ServiceModel.Grpc.Configuration
{
    /// <summary>
    /// Represents a type which provides support to create <see cref="Marshaller{T}"/>.
    /// </summary>
    public interface IMarshallerFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
        Marshaller<T> CreateMarshaller<T>();
    }
}
