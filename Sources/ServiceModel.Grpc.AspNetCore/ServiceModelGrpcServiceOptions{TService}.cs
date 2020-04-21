//// ReSharper disable CheckNamespace

using ServiceModel.Grpc.Configuration;

namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a configuration for a specific ServiceModel.Grpc service.
    /// </summary>
    /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
    public sealed class ServiceModelGrpcServiceOptions<TService>
        where TService : class
    {
        /// <summary>
        /// Gets or sets a factory for serializing and deserializing messages.
        /// </summary>
        public IMarshallerFactory MarshallerFactory { get; set; }
    }
}
