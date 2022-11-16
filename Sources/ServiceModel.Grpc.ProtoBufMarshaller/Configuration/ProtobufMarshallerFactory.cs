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

using System;
using Grpc.Core;
using ProtoBuf;
using ProtoBuf.Meta;
using SerializationContext = Grpc.Core.SerializationContext;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="Serializer"/>.
/// </summary>
public sealed class ProtobufMarshallerFactory : IMarshallerFactory
{
    /// <summary>
    /// Default instance of <see cref="ProtobufMarshallerFactory"/> with <see cref="ProtoBuf.Meta.RuntimeTypeModel"/>.Default.
    /// </summary>
    public static readonly IMarshallerFactory Default = new ProtobufMarshallerFactory();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufMarshallerFactory"/> class.
    /// </summary>
    public ProtobufMarshallerFactory()
        : this(RuntimeTypeModel.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufMarshallerFactory"/> class with specific <see cref="ProtoBuf.Meta.RuntimeTypeModel"/>.
    /// </summary>
    /// <param name="runtimeTypeModel">The <see cref="ProtoBuf.Meta.RuntimeTypeModel"/>.</param>
    public ProtobufMarshallerFactory(RuntimeTypeModel runtimeTypeModel)
    {
        if (runtimeTypeModel == null)
        {
            throw new ArgumentNullException(nameof(runtimeTypeModel));
        }

        RuntimeTypeModel = runtimeTypeModel;
    }

    /// <summary>
    /// Gets the <see cref="ProtoBuf.Meta.RuntimeTypeModel"/>.
    /// </summary>
    public RuntimeTypeModel RuntimeTypeModel { get; }

    /// <summary>
    /// Creates the <see cref="Marshaller{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
    public Marshaller<T> CreateMarshaller<T>()
    {
        if (ReferenceEquals(RuntimeTypeModel, RuntimeTypeModel.Default))
        {
            return ProtobufMarshaller<T>.Default;
        }

        return new Marshaller<T>(Serialize, Deserialize<T>);
    }

    internal static void Serialize<T>(T value, SerializationContext context, RuntimeTypeModel runtimeTypeModel)
    {
        runtimeTypeModel.Serialize(context.GetBufferWriter(), value);
        context.Complete();
    }

    internal static T Deserialize<T>(DeserializationContext context, RuntimeTypeModel runtimeTypeModel)
    {
        return runtimeTypeModel.Deserialize<T>(context.PayloadAsReadOnlySequence());
    }

    private void Serialize<T>(T value, SerializationContext context) => Serialize(value, context, RuntimeTypeModel!);

    private T Deserialize<T>(DeserializationContext context) => Deserialize<T>(context, RuntimeTypeModel!);
}