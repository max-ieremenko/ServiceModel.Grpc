// <copyright>
// Copyright 2021 Max Ieremenko
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
using Google.Protobuf;
using Grpc.Core;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.Api
{
    internal sealed class GoogleProtoMarshallerFactory : IMarshallerFactory
    {
        public static readonly IMarshallerFactory Default = new GoogleProtoMarshallerFactory();

        public Marshaller<T> CreateMarshaller<T>()
        {
            Action<T, SerializationContext> serializer = Serialize;
            Func<DeserializationContext, T> deserializer = Deserialize<T>;
            return new Marshaller<T>(serializer, deserializer);
        }

        private static void Serialize<T>(T value, SerializationContext context)
        {
            var message = (Message<SomeObjectProto>)((object)value);
            context.SetPayloadLength(message.Value1.CalculateSize());
            message.Value1.WriteTo(context.GetBufferWriter());
            context.Complete();
        }

        private static T Deserialize<T>(DeserializationContext context)
        {
            var value = SomeObjectProto.Parser.ParseFrom(context.PayloadAsReadOnlySequence());
            object result = new Message<SomeObjectProto>(value);
            return (T)result;
        }
    }
}
