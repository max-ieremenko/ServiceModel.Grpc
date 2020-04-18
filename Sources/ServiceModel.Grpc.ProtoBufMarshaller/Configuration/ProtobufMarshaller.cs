using Grpc.Core;
using ProtoBuf.Meta;

namespace ServiceModel.Grpc.Configuration
{
    internal sealed class ProtobufMarshaller<T>
    {
        public static readonly Marshaller<T> Default = new Marshaller<T>(Serialize, Deserialize);

        private static byte[] Serialize(T value) => ProtobufMarshallerFactory.Serialize(value, RuntimeTypeModel.Default);

        private static T Deserialize(byte[] value) => ProtobufMarshallerFactory.Deserialize<T>(value, RuntimeTypeModel.Default);
    }
}
