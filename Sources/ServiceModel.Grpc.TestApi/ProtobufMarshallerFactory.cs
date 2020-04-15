using System.IO;
using Grpc.Core;
using ProtoBuf;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.TestApi
{
    public sealed class ProtobufMarshallerFactory : IMarshallerFactory
    {
        public static readonly IMarshallerFactory Default = new ProtobufMarshallerFactory();

        public Marshaller<T> CreateMarshaller<T>() => ProtobufMarshaller<T>.Default;

        private sealed class ProtobufMarshaller<T>
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
                    Serializer.Serialize(buffer, value);
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
                {
                    return Serializer.Deserialize<T>(buffer);
                }
            }
        }
    }
}
