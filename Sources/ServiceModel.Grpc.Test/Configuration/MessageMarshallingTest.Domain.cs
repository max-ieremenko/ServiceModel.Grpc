using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Grpc.Core;
using Newtonsoft.Json;
using ProtoBuf;

namespace ServiceModel.Grpc.Configuration
{
    public partial class MessageMarshallingTest
    {
        [DataContract]
        public class Person
        {
            [DataMember(Order = 1)]
            public string Name { get; set; }

            [DataMember(Order = 2)]
            public PersonAddress Address { get; set; }
        }

        [DataContract]
        public class PersonAddress
        {
            [DataMember(Order = 1)]
            public string Street { get; set; }
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

        public sealed class JsonMarshaller<T>
        {
            public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

            private static byte[] Serialize(T value)
            {
                if (value == null)
                {
                    return null;
                }

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
                if (value == null)
                {
                    return default;
                }

                using (var buffer = new MemoryStream(value))
                using (var reader = new JsonTextReader(new StreamReader(buffer)))
                {
                    var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    return serializer.Deserialize<T>(reader);
                }
            }
        }
    }
}
