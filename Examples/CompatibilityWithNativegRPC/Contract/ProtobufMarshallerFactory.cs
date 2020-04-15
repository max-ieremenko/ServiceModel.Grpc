using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace Contract
{
    public sealed class ProtobufMarshallerFactory : IMarshallerFactory
    {
        public static IMarshallerFactory Default = new ProtobufMarshallerFactory();

        public Marshaller<T> CreateMarshaller<T>() => ProtobufMarshaller<T>.Default;
    }
}
