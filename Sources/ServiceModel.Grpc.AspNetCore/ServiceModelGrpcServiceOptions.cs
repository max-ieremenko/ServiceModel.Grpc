using ServiceModel.Grpc.Configuration;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a default configuration for all ServiceModel.Grpc services.
    /// </summary>
    public sealed class ServiceModelGrpcServiceOptions
    {
        /// <summary>
        /// Gets or sets default factory for serializing and deserializing messages.
        /// </summary>
        public IMarshallerFactory DefaultMarshallerFactory { get; set; }
    }
}
