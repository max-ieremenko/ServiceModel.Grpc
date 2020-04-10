using ServiceModel.Grpc.Configuration;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    public sealed class ServiceModelGrpcServiceOptions
    {
        public IMarshallerFactory DefaultMarshallerFactory { get; set; }
    }
}
