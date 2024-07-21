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
using MessagePack;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.MarshallerTest;

internal static class MessagePackTest
{
    public static Marshaller<T> CreateDefaultMarshaller<T>() => new Marshaller<T>(SerializeDefault, DeserializeDefault<T>);

    public static Marshaller<T> CreateStreamMarshaller<T>() => new Marshaller<T>(SerializeStream, DeserializeStream<T>);

    private static void SerializeStream<T>(T value, SerializationContext context)
    {
        using (var stream = context.AsStream())
        {
            MessagePackSerializer.Serialize(stream, value);
        }

        context.Complete();
    }

    private static T DeserializeStream<T>(DeserializationContext context)
    {
        using (var stream = context.AsStream())
        {
            return MessagePackSerializer.Deserialize<T>(stream);
        }
    }

    private static void SerializeDefault<T>(T value, SerializationContext context)
    {
        MessagePackSerializer.Serialize(context.GetBufferWriter(), value, MessagePackSerializer.DefaultOptions);
        context.Complete();
    }

    private static T DeserializeDefault<T>(DeserializationContext context)
    {
        return MessagePackSerializer.Deserialize<T>(context.PayloadAsReadOnlySequence(), MessagePackSerializer.DefaultOptions);
    }
}