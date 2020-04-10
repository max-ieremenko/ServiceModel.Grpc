using System.IO;
using System.Runtime.Serialization;
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    internal static class DataContractMarshaller<T>
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
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(buffer, value);

                return buffer.ToArray();
            }
        }

        private static T Deserialize(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return default;
            }

            using (var buffer = new MemoryStream(value))
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(buffer);
            }
        }
    }
}
