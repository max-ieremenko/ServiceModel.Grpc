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
using Nerdbank.MessagePack;
using PolyType;
using PolyType.Abstractions;
using ServiceModel.Grpc.Configuration.Internal;
using SerializationContext = Grpc.Core.SerializationContext;

#pragma warning disable ServiceModelGrpcInternalAPI

// Use an overload that does not take an ITypeShape<T> or ITypeShapeProvider
#pragma warning disable NBMsgPack051

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="MessagePackSerializer"/>.
/// </summary>
public sealed class NerdbankMessagePackMarshallerFactory : IMarshallerFactory
{
    private MessageTypeShapeProvider _shapeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NerdbankMessagePackMarshallerFactory"/> class with ITypeShapeProvider and default MessagePackSerializer.
    /// </summary>
    /// <param name="typeShapeProvider">The <see cref="ITypeShapeProvider"/>.</param>
    public NerdbankMessagePackMarshallerFactory(ITypeShapeProvider typeShapeProvider)
        : this(CreateDefaultSerializer(), typeShapeProvider)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NerdbankMessagePackMarshallerFactory"/> class with MessagePackSerializer and ITypeShapeProvider.
    /// </summary>
    /// <param name="serializer">The <see cref="MessagePackSerializer"/>.</param>
    /// <param name="typeShapeProvider">The <see cref="ITypeShapeProvider"/>.</param>
    public NerdbankMessagePackMarshallerFactory(MessagePackSerializer serializer, ITypeShapeProvider typeShapeProvider)
    {
        GrpcPreconditions.CheckNotNull(serializer, nameof(serializer));
        GrpcPreconditions.CheckNotNull(typeShapeProvider, nameof(typeShapeProvider));

        _shapeProvider = new MessageTypeShapeProvider(NerdbankMessagePackMarshaller.Cache, typeShapeProvider);

        Serializer = serializer;
    }

    /// <summary>
    /// Gets the instance of <see cref="MessagePackSerializer"/> used by this factory.
    /// </summary>
    public MessagePackSerializer Serializer { get; }

    /// <summary>
    /// Gets the instance of <see cref="ITypeShapeProvider"/> used by this factory.
    /// </summary>
    public ITypeShapeProvider TypeShapeProvider => _shapeProvider.UserProvider;

#if !NETSTANDARD2_0
    /// <summary>
    /// Creates a new instance of the <see cref="NerdbankMessagePackMarshallerFactory"/> with ITypeShapeProvider linked to TShapeable.
    /// </summary>
    /// <typeparam name="TShapeable">The TShapeable instance to get it`s provider.</typeparam>
    /// <returns>NerdbankMessagePackMarshallerFactory instance.</returns>
    public static NerdbankMessagePackMarshallerFactory CreateWithTypeShapeProviderFrom<TShapeable>()
        where TShapeable : IShapeable<TShapeable> =>
        CreateWithTypeShapeProviderFrom<TShapeable>(CreateDefaultSerializer());

    /// <summary>
    /// Creates a new instance of the <see cref="NerdbankMessagePackMarshallerFactory"/> with ITypeShapeProvider linked to TShapeable.
    /// </summary>
    /// <typeparam name="TShapeable">The TShapeable instance to get it`s provider.</typeparam>
    /// <param name="serializer">The <see cref="MessagePackSerializer"/>.</param>
    /// <returns>NerdbankMessagePackMarshallerFactory instance.</returns>
    public static NerdbankMessagePackMarshallerFactory CreateWithTypeShapeProviderFrom<TShapeable>(MessagePackSerializer serializer)
        where TShapeable : IShapeable<TShapeable> =>
        new(serializer, TShapeable.GetTypeShape().Provider);
#endif

    /// <summary>
    /// Creates a new instance of the <see cref="MessagePackSerializer"/> with PerfOverSchemaStability = true.
    /// </summary>
    /// <returns>MessagePackSerializer instance.</returns>
    public static MessagePackSerializer CreateDefaultSerializer() => new();

    /// <summary>
    /// Creates the <see cref="Marshaller{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
    public Marshaller<T> CreateMarshaller<T>() => new(Serialize, Deserialize<T>);

    internal ITypeShape<T> GetShape<T>() => _shapeProvider.GetShape<T>();

    private void Serialize<T>(T value, SerializationContext context)
    {
        Serializer.Serialize(context.GetBufferWriter(), value, GetShape<T>());
        context.Complete();
    }

    private T Deserialize<T>(DeserializationContext context) =>
        Serializer.Deserialize(context.PayloadAsReadOnlySequence(), GetShape<T>())!;
}