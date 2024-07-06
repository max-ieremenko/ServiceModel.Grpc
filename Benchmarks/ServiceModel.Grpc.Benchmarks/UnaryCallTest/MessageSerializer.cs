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

using System.Buffers.Binary;
using Google.Protobuf;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest;

internal static class MessageSerializer
{
    public static byte[] Create(IMessage message) => Create(message.ToByteArray());

    public static byte[] Create<TMessage>(IMarshallerFactory marshallerFactory, TMessage message)
    {
        var marshaller = marshallerFactory.CreateMarshaller<TMessage>();
        var payload = MarshallerExtensions.Serialize(marshaller, message);
        return Create(payload);
    }

    private static byte[] Create(byte[] payload)
    {
        var result = new byte[payload.Length + 5];

        // not compressed
        result[0] = 0;

        // length
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(1), (uint)payload.Length);

        // payload
        Array.Copy(payload, 0, result, 5, payload.Length);

        return result;
    }
}