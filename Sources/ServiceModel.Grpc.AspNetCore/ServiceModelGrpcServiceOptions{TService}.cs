//// ReSharper disable CheckNamespace

using ServiceModel.Grpc.Configuration;

namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    public sealed class ServiceModelGrpcServiceOptions<TService>
        where TService : class
    {
        public IMarshallerFactory MarshallerFactory { get; set; }
    }
}
