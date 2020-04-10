using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    public interface IMarshallerFactory
    {
        Marshaller<T> CreateMarshaller<T>();
    }
}
