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
using MessagePack.Formatters;
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
public static class MessagePackMarshaller
{
    /// <summary>
    /// Register the formatter for <see cref="Message{T1}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    public static void RegisterMessageFormatter<T1>()
    {
        if (!MessagePackFormatterCache<Message<T1>>.IsRegistered)
        {
            MessagePackFormatterCache<Message<T1>?>.Formatter = new MessageMessagePackFormatter<T1>();
        }
    }

    /// <summary>
    /// Register the formatter for <see cref="Message{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1">T1 type.</typeparam>
    /// <typeparam name="T2">T2 type.</typeparam>
    public static void RegisterMessageFormatter<T1, T2>()
    {
        if (!MessagePackFormatterCache<Message<T1, T2>>.IsRegistered)
        {
            MessagePackFormatterCache<Message<T1, T2>?>.Formatter = new MessageMessagePackFormatter<T1, T2>();
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
        if (!MessagePackFormatterCache<Message<T1, T2, T3>>.IsRegistered)
        {
            MessagePackFormatterCache<Message<T1, T2, T3>?>.Formatter = new MessageMessagePackFormatter<T1, T2, T3>();
        }
    }

    /// <summary>
    /// Register the formatter for generated <see cref="Message"/>.
    /// </summary>
    /// <typeparam name="TMessage">Message type.</typeparam>
    /// <typeparam name="TFormatter">Formatter type.</typeparam>
    public static void RegisterFormatter<TMessage, TFormatter>()
        where TFormatter : IMessagePackFormatter<TMessage?>, new()
    {
        if (!MessagePackFormatterCache<TMessage>.IsRegistered)
        {
            MessagePackFormatterCache<TMessage>.Formatter = new TFormatter();
        }
    }

    internal static void RegisterDefaults()
    {
        if (!MessagePackFormatterCache<Message>.IsRegistered)
        {
            MessagePackFormatterCache<Message?>.Formatter = new MessageMessagePackFormatter();
        }
    }
}