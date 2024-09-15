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

using System.Buffers;
using Grpc.Core;
using Grpc.Core.Utils;
using ServiceModel.Grpc.Configuration.IO;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides set of helpers for <see cref="Marshaller{T}"/> and <see cref="IMarshallerFactory"/>.
/// </summary>
public static class MarshallerExtensions
{
    /// <summary>
    /// Serialize a value into byte array.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="marshaller">The <see cref="Marshaller{T}"/> instance to serialize with.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>Payload created by the <see cref="Marshaller{T}"/> instance.</returns>
    public static byte[] Serialize<T>(Marshaller<T> marshaller, T value)
    {
        GrpcPreconditions.CheckNotNull(marshaller, nameof(marshaller));

        byte[] payload;
        using (var serializationContext = new DefaultSerializationContext())
        {
            marshaller.ContextualSerializer(value, serializationContext);
            payload = serializationContext.GetContent();
        }

        return payload;
    }

    /// <summary>
    /// Deserialize a value from byte array.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="marshaller">The <see cref="Marshaller{T}"/> instance to deserialize with.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>A value deserialized by the <see cref="Marshaller{T}"/> instance.</returns>
    public static T Deserialize<T>(Marshaller<T> marshaller, byte[] payload)
    {
        GrpcPreconditions.CheckNotNull(marshaller, nameof(marshaller));
        GrpcPreconditions.CheckNotNull(payload, nameof(payload));

        return marshaller.ContextualDeserializer(new DefaultDeserializationContext(payload));
    }

    /// <summary>
    /// Deserialize a value from byte array.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="marshaller">The <see cref="Marshaller{T}"/> instance to deserialize with.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>A value deserialized by the <see cref="Marshaller{T}"/> instance.</returns>
    public static T Deserialize<T>(Marshaller<T> marshaller, in ReadOnlySequence<byte> payload)
    {
        GrpcPreconditions.CheckNotNull(marshaller, nameof(marshaller));

        return marshaller.ContextualDeserializer(new DefaultDeserializationContext(payload));
    }

    /// <summary>
    /// Serialize a value into byte array.
    /// </summary>
    /// <param name="factory">The <see cref="IMarshallerFactory"/> instance to serialize with.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>Payload created by the <see cref="IMarshallerFactory"/> instance.</returns>
    [RequiresDynamicCode("The native code for the serialization might not be available at runtime.")]
    [RequiresUnreferencedCode("The trimming may not validate that the requirements of 'value' are met.")]
    public static byte[] SerializeObject(IMarshallerFactory factory, object value)
    {
        GrpcPreconditions.CheckNotNull(factory, nameof(factory));
        GrpcPreconditions.CheckNotNull(value, nameof(value));

        if (value is byte[] buffer)
        {
            return buffer;
        }

        return MarshallerSerializers.Get(value.GetType()).Serialize(factory, value);
    }

    /// <summary>
    /// Deserialize a value from byte array.
    /// </summary>
    /// <param name="factory">The <see cref="IMarshallerFactory"/> instance to deserialize with.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>A value deserialized by the <see cref="IMarshallerFactory"/> instance.</returns>
    [RequiresDynamicCode("The native code for the deserialization might not be available at runtime.")]
    [RequiresUnreferencedCode("The trimming may not validate that the requirements of 'valueType' are met.")]
    public static object DeserializeObject(IMarshallerFactory factory, Type valueType, byte[] payload)
    {
        GrpcPreconditions.CheckNotNull(factory, nameof(factory));
        GrpcPreconditions.CheckNotNull(valueType, nameof(valueType));
        GrpcPreconditions.CheckNotNull(payload, nameof(payload));

        if (valueType == typeof(byte[]))
        {
            return payload;
        }

        return MarshallerSerializers.Get(valueType).Deserialize(factory, payload);
    }

    /// <summary>
    /// Deserialize a value from <see cref="ReadOnlySequence{Byte}"/>.
    /// </summary>
    /// <param name="factory">The <see cref="IMarshallerFactory"/> instance to deserialize with.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>A value deserialized by the <see cref="IMarshallerFactory"/> instance.</returns>
    [RequiresDynamicCode("The native code for the deserialization might not be available at runtime.")]
    [RequiresUnreferencedCode("The trimming may not validate that the requirements of 'valueType' are met.")]
    public static object DeserializeObject(IMarshallerFactory factory, Type valueType, in ReadOnlySequence<byte> payload)
    {
        GrpcPreconditions.CheckNotNull(factory, nameof(factory));
        GrpcPreconditions.CheckNotNull(valueType, nameof(valueType));
        GrpcPreconditions.CheckNotNull(payload, nameof(payload));

        if (valueType == typeof(byte[]))
        {
            return payload.ToArray();
        }

        return MarshallerSerializers.Get(valueType).Deserialize(factory, payload);
    }
}