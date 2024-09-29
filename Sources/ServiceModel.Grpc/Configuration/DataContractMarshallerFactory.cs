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

using System.Runtime.Serialization;
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="DataContractSerializer"/>.
/// </summary>
[RequiresDynamicCode("The System.Runtime.Serialization.DataContractSerializer might require types that cannot be statically analyzed.")]
[RequiresUnreferencedCode("The System.Runtime.Serialization.DataContractSerializer might require types that cannot be statically analyzed.")]
public sealed class DataContractMarshallerFactory : IMarshallerFactory
{
    /// <summary>
    /// Default instance of <see cref="DataContractMarshallerFactory"/>.
    /// </summary>
    public static readonly IMarshallerFactory Default = new DataContractMarshallerFactory();

    /// <summary>
    /// Creates the <see cref="Marshaller{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
    public Marshaller<T> CreateMarshaller<T>()
    {
        if (Features.IsDataContractMarshallerDisabled)
        {
            throw new NotSupportedException("DataContractMarshallerFactory serialization and deserialization are disabled within this application.");
        }

        return DataContractMarshaller<T>.Default;
    }
}