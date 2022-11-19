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
using System.Buffers.Binary;
using System.Reflection;
using Google.Protobuf;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest;

internal static class MessageSerializer
{
    private static readonly Func<IMarshallerFactory, object, byte[]> MarshallerSerialize = ResolveMarshallerSerialize();

    public static byte[] Create(IMessage data) => Create(data.ToByteArray());

    public static byte[] Create(IMarshallerFactory marshallerFactory, object data) => Create(MarshallerSerialize(marshallerFactory, data));

    private static byte[] Create(byte[] data)
    {
        var result = new byte[data.Length + 5];

        // not compressed
        result[0] = 0;

        // length
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(1), (uint)data.Length);

        // data
        Array.Copy(data, 0, result, 5, data.Length);

        return result;
    }

    private static Func<IMarshallerFactory, object, byte[]> ResolveMarshallerSerialize()
    {
        return typeof(IMarshallerFactory)
            .Assembly
            .GetType("ServiceModel.Grpc.Configuration.MarshallerFactoryExtensions", true, false)
            .GetMethod("SerializeHeader", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .CreateDelegate<Func<IMarshallerFactory, object, byte[]>>();
    }
}