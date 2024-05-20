// <copyright>
// Copyright 2021-2023 Max Ieremenko
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

namespace ServiceModel.Grpc.Benchmarks.Api;

internal sealed class GoogleProtoMarshallerFactory : IMarshallerFactory
{
    public static readonly IMarshallerFactory Default = new GoogleProtoMarshallerFactory();

    private readonly object _someObjectProtoMarshaller = Marshallers.Create(Serialize, Deserialize);

    public Marshaller<T> CreateMarshaller<T>()
    {
        if (typeof(T) == typeof(Message<SomeObjectProto>))
        {
            return (Marshaller<T>)_someObjectProtoMarshaller;
        }

        return Marshallers.Create(FailSerialize, FailDeserialize<T>);
    }

    private static void Serialize(Message<SomeObjectProto> value, SerializationContext context)
    {
        var message = value.Value1!;
        context.SetPayloadLength(message.CalculateSize());
        message.WriteTo(context.GetBufferWriter());
        context.Complete();
    }

    private static Message<SomeObjectProto> Deserialize(DeserializationContext context)
    {
        var value = SomeObjectProto.Parser.ParseFrom(context.PayloadAsReadOnlySequence());
        return new Message<SomeObjectProto>(value);
    }

    private static T FailDeserialize<T>(DeserializationContext value) => throw new NotImplementedException();

    private static void FailSerialize<T>(T value, SerializationContext context) => throw new NotImplementedException();
}