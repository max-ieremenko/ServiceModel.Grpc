using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    public sealed class DataContractMarshallerFactory : IMarshallerFactory
    {
        public static readonly IMarshallerFactory Default = new DataContractMarshallerFactory();

        public Marshaller<T> CreateMarshaller<T>() => DataContractMarshaller<T>.Default;
    }
}
