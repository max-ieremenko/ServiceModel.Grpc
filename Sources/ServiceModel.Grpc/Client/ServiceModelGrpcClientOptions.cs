using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Client
{
    public sealed class ServiceModelGrpcClientOptions
    {
        public IMarshallerFactory MarshallerFactory { get; set; }
    }
}
