﻿// <copyright>
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

using System.Runtime.Serialization;
using Grpc.Core;
using Newtonsoft.Json;
using ProtoBuf;

namespace ServiceModel.Grpc.Emit.Configuration;

public partial class MessageMarshallingTest
{
    [DataContract]
    public record Person
    {
        [DataMember(Order = 1)]
        public string? Name { get; set; }

        [DataMember(Order = 2)]
        public PersonAddress? Address { get; set; }
    }

    [DataContract]
    public record PersonAddress
    {
        [DataMember(Order = 1)]
        public string? Street { get; set; }
    }

    [DataContract]
    [KnownType(typeof(Sword))]
    [KnownType(typeof(Knife))]
    [ProtoInclude(3, typeof(Sword))]
    [ProtoInclude(4, typeof(Knife))]
    public abstract record Weapon
    {
        [DataMember(Order = 1)]
        public int HitDamage { get; set; }
    }

    [DataContract]
    public record Sword : Weapon
    {
        [DataMember(Order = 1)]
        public int Length { get; set; }
    }

    [DataContract]
    public record Knife : Weapon;

    [DataContract]
    ////[KnownType(typeof(DynamicObject))]
    public class DynamicObject
    {
        [DataMember(Order = 1)]
        public List<object> Values { get; private set; } = new();

        public override bool Equals(object? other) => other is DynamicObject obj && Values.SequenceEqual(obj.Values);

        public override int GetHashCode() => 0;
    }

    [Serializable]
    public record TheContainer<T> : ISerializable
    {
        public TheContainer()
        {
        }

        public TheContainer(T value)
        {
            Value = value;
        }

        private TheContainer(SerializationInfo info, StreamingContext context)
        {
            Value = (T)info.GetValue(nameof(Value), typeof(T))!;
        }

        public T Value { get; set; } = default!;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Value), Value);
        }
    }

    public sealed class JsonMarshaller<T>
    {
        public static readonly Marshaller<T> Default = new(Serialize, Deserialize);

        private static byte[] Serialize(T value)
        {
            using (var buffer = new MemoryStream())
            {
                using (var writer = new StreamWriter(buffer, Encoding.Unicode, 1024, true))
                {
                    var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    serializer.Serialize(writer, value);
                }

                return buffer.ToArray();
            }
        }

        private static T Deserialize(byte[] value)
        {
            using (var buffer = new MemoryStream(value))
            using (var reader = new JsonTextReader(new StreamReader(buffer)))
            {
                var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                return serializer.Deserialize<T>(reader)!;
            }
        }
    }
}