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

using Grpc.Core;
using Grpc.Core.Utils;
using MessagePack;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="MessagePackSerializer"/>.
/// </summary>
public sealed class MessagePackMarshallerFactory : IMarshallerFactory
{
    /// <summary>
    /// Default instance of <see cref="MessagePackMarshallerFactory"/> with <see cref="MessagePackSerializer"/>.DefaultOptions.
    /// </summary>
    public static readonly IMarshallerFactory Default = new MessagePackMarshallerFactory();

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackMarshallerFactory"/> class with MessagePackSerializer default options.
    /// </summary>
    public MessagePackMarshallerFactory()
        : this(MessagePackSerializer.DefaultOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackMarshallerFactory"/> class with specific <see cref="MessagePackSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The <see cref="MessagePackSerializerOptions"/>.</param>
    public MessagePackMarshallerFactory(MessagePackSerializerOptions options)
    {
        Options = GrpcPreconditions.CheckNotNull(options, nameof(options));
    }

    /// <summary>
    /// Gets the <see cref="MessagePackSerializerOptions"/>.
    /// </summary>
    public MessagePackSerializerOptions Options { get; }

    /// <summary>
    /// Creates the <see cref="Marshaller{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
    public Marshaller<T> CreateMarshaller<T>()
    {
        if (ReferenceEquals(Options, MessagePackSerializer.DefaultOptions))
        {
            return MessagePackMarshaller<T>.Default;
        }

        return new Marshaller<T>(Serialize, Deserialize<T>);
    }

    internal static void Serialize<T>(T value, SerializationContext context, MessagePackSerializerOptions options)
    {
        MessagePackSerializer.Serialize(context.GetBufferWriter(), value, options);
        context.Complete();
    }

    internal static T Deserialize<T>(DeserializationContext context, MessagePackSerializerOptions options)
    {
        return MessagePackSerializer.Deserialize<T>(context.PayloadAsReadOnlySequence(), options);
    }

    private void Serialize<T>(T value, SerializationContext context) => Serialize(value, context, Options);

    private T Deserialize<T>(DeserializationContext context) => Deserialize<T>(context, Options);
}