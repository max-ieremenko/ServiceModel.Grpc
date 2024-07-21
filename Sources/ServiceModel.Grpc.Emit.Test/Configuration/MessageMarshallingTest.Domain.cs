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

using System.Runtime.Serialization;
using Grpc.Core;
using Newtonsoft.Json;
using ProtoBuf;

namespace ServiceModel.Grpc.Emit.Configuration;

public partial class MessageMarshallingTest
{
    [DataContract]
    public class Person
    {
        [DataMember(Order = 1)]
        public string? Name { get; set; }

        [DataMember(Order = 2)]
        public PersonAddress? Address { get; set; }
    }

    [DataContract]
    public class PersonAddress
    {
        [DataMember(Order = 1)]
        public string? Street { get; set; }
    }

    [DataContract]
    [KnownType(typeof(Sword))]
    [KnownType(typeof(Knife))]
    [ProtoInclude(3, typeof(Sword))]
    [ProtoInclude(4, typeof(Knife))]
    public abstract class Weapon
    {
        [DataMember(Order = 1)]
        public int HitDamage { get; set; }
    }

    [DataContract]
    public class Sword : Weapon
    {
        [DataMember(Order = 1)]
        public int Length { get; set; }
    }

    [DataContract]
    public class Knife : Weapon
    {
    }

    [DataContract]
    ////[KnownType(typeof(DynamicObject))]
    public class DynamicObject
    {
        [DataMember(Order = 1)]
        public List<object> Values { get; private set; } = new List<object>();
    }

    [Serializable]
    public class TheContainer<T> : ISerializable
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

        public override bool Equals(object? obj)
        {
            if (obj is TheContainer<T> other)
            {
                return EqualityComparer<T>.Default.Equals(Value, other.Value);
            }

            return false;
        }

        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
    }

    public sealed class JsonMarshaller<T>
    {
        public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

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