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

using System.ComponentModel;
using PolyType;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration.Internal;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Experimental("ServiceModelGrpcInternalAPI")]
public static class NerdbankMessagePackMarshaller
{
    internal static readonly MessageTypeShapeCache Cache = new();

    /// <summary>
    /// Register the <see cref="ITypeShape"/> for <see cref="Message{T1}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    public static void RegisterMessageShape<T1>()
    {
        if (!IsRegisteredMessage<Message<T1>>())
        {
            NewMessageShapeBuilder<Message<T1>>(1)
                .AddProperty(MessageProperty.M1<T1>.Get1, MessageProperty.M1<T1>.Set1)
                .Register();
        }
    }

    /// <summary>
    /// Register the <see cref="ITypeShape"/> for <see cref="Message{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    /// <typeparam name="T2">T2 type.</typeparam>
    public static void RegisterMessageShape<T1, T2>()
    {
        if (!IsRegisteredMessage<Message<T1, T2>>())
        {
            NewMessageShapeBuilder<Message<T1, T2>>(2)
                .AddProperty(MessageProperty.M2<T1, T2>.Get1, MessageProperty.M2<T1, T2>.Set1)
                .AddProperty(MessageProperty.M2<T1, T2>.Get2, MessageProperty.M2<T1, T2>.Set2)
                .Register();
        }
    }

    /// <summary>
    /// Register the <see cref="ITypeShape"/> for <see cref="Message{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    /// <typeparam name="T2">T2 type.</typeparam>
    /// <typeparam name="T3">T3 type.</typeparam>
    public static void RegisterMessageShape<T1, T2, T3>()
    {
        if (!IsRegisteredMessage<Message<T1, T2, T3>>())
        {
            NewMessageShapeBuilder<Message<T1, T2, T3>>(3)
                .AddProperty(MessageProperty.M3<T1, T2, T3>.Get1, MessageProperty.M3<T1, T2, T3>.Set1)
                .AddProperty(MessageProperty.M3<T1, T2, T3>.Get2, MessageProperty.M3<T1, T2, T3>.Set2)
                .AddProperty(MessageProperty.M3<T1, T2, T3>.Get3, MessageProperty.M3<T1, T2, T3>.Set3)
                .Register();
        }
    }

    /// <summary>
    /// Check if a <see cref="ITypeShape"/> is registered for <see cref="Message"/>.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <returns>True if a shape is registered.</returns>
    public static bool IsRegisteredMessage<TMessage>() => Cache.IsRegistered<TMessage>();

    /// <summary>
    /// Register the <see cref="ITypeShape"/> for generated <see cref="Message"/>.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <param name="propertiesCount">Number of properties.</param>
    /// <returns>The shape builder.</returns>
    public static IMessageShapeBuilder<TMessage> NewMessageShapeBuilder<TMessage>(int propertiesCount)
        where TMessage : new() =>
        new MessageShapeBuilder<TMessage>(propertiesCount, Cache);
}