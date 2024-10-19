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
using System.Runtime.CompilerServices;
using MemoryPack;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration.Formatters;

namespace ServiceModel.Grpc.Configuration;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Experimental("ServiceModelGrpcInternalAPI")]
public static class MemoryPackMarshaller
{
    /// <summary>
    /// Register the formatter for <see cref="Message{T1}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    public static void RegisterMessageFormatter<T1>()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<MessageMemoryPackFormatter<T1>>())
        {
            MemoryPackFormatterProvider.Register(new MessageMemoryPackFormatter<T1>());
        }
    }

    /// <summary>
    /// Register the formatter for <see cref="Message{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    /// <typeparam name="T2">T2 type.</typeparam>
    public static void RegisterMessageFormatter<T1, T2>()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<MessageMemoryPackFormatter<T1, T2>>())
        {
            MemoryPackFormatterProvider.Register(new MessageMemoryPackFormatter<T1, T2>());
        }
    }

    /// <summary>
    /// Register the formatter for <see cref="Message{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    /// <typeparam name="T2">T2 type.</typeparam>
    /// <typeparam name="T3">T3 type.</typeparam>
    public static void RegisterMessageFormatter<T1, T2, T3>()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<MessageMemoryPackFormatter<T1, T2, T3>>())
        {
            MemoryPackFormatterProvider.Register(new MessageMemoryPackFormatter<T1, T2, T3>());
        }
    }

    /// <summary>
    /// Register the formatter for generated <see cref="Message"/>.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <typeparam name="TFormatter">Formatter type.</typeparam>
    public static void RegisterFormatter<TMessage, TFormatter>()
        where TFormatter : MemoryPackFormatter<TMessage>, new()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<TFormatter>())
        {
            MemoryPackFormatterProvider.Register(new TFormatter());

            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                MemoryPackFormatterProvider.RegisterGenericType(typeof(TMessage), typeof(TFormatter));
            }
        }
    }

    internal static void RegisterDefaults()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<MessageMemoryPackFormatter>())
        {
            MemoryPackFormatterProvider.Register(new MessageMemoryPackFormatter());

            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                MemoryPackFormatterProvider.RegisterGenericType(typeof(Message<>), typeof(MessageMemoryPackFormatter<>));
                MemoryPackFormatterProvider.RegisterGenericType(typeof(Message<,>), typeof(MessageMemoryPackFormatter<,>));
                MemoryPackFormatterProvider.RegisterGenericType(typeof(Message<,,>), typeof(MessageMemoryPackFormatter<,,>));
            }
        }
    }
}