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
using MemoryPack;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="MemoryPackSerializer"/>.
/// </summary>
public sealed class MemoryPackMarshallerFactory : IMarshallerFactory
{
    /// <summary>
    /// Default instance of <see cref="MemoryPackMarshallerFactory"/> with <see cref="MemoryPackSerializer"/>.DefaultOptions.
    /// </summary>
    public static readonly MemoryPackMarshallerFactory Default = new();

    private readonly bool _defaultOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPackMarshallerFactory"/> class.
    /// </summary>
    public MemoryPackMarshallerFactory()
        : this(MemoryPackSerializerOptions.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPackMarshallerFactory"/> class with specific <see cref="MemoryPackSerializerOptions"/>.
    /// </summary>
    /// <param name="options">The <see cref="MemoryPackSerializerOptions"/>.</param>
    public MemoryPackMarshallerFactory(MemoryPackSerializerOptions options)
    {
        Options = GrpcPreconditions.CheckNotNull(options, nameof(options));

        _defaultOptions = Options.Equals(MemoryPackSerializerOptions.Default);
        MemoryPackMarshaller.RegisterDefaults();
    }

    /// <summary>
    /// Gets the <see cref="MemoryPackSerializerOptions"/>.
    /// </summary>
    public MemoryPackSerializerOptions Options { get; }

    /// <summary>
    /// Creates the <see cref="Marshaller{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
    public Marshaller<T> CreateMarshaller<T>()
    {
        if (_defaultOptions)
        {
            return MemoryPackMarshaller<T>.Default;
        }

        return new Marshaller<T>(Serialize, Deserialize<T>);
    }

    internal static void Serialize<T>(T value, SerializationContext context, MemoryPackSerializerOptions options)
    {
        MemoryPackSerializer.Serialize(context.GetBufferWriter(), value, options);
        context.Complete();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2091:DynamicallyAccessedMemberTypes.All")]
    internal static T Deserialize<T>(DeserializationContext context, MemoryPackSerializerOptions options) =>
        MemoryPackSerializer.Deserialize<T>(context.PayloadAsReadOnlySequence(), options)!;

    private void Serialize<T>(T value, SerializationContext context) => Serialize(value, context, Options);

    private T Deserialize<T>(DeserializationContext context) => Deserialize<T>(context, Options);
}