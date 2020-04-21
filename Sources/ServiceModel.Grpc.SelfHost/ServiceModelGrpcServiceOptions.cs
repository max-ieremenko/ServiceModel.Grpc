using System;
using Grpc.Core.Logging;
using ServiceModel.Grpc.Configuration;

//// ReSharper disable CheckNamespace
namespace Grpc.Core
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a configuration for a ServiceModel.Grpc services.
    /// </summary>
    public sealed class ServiceModelGrpcServiceOptions
    {
        /// <summary>
        /// Gets or sets a factory for serializing and deserializing messages.
        /// </summary>
        public IMarshallerFactory MarshallerFactory { get; set; }

        /// <summary>
        /// Gets or sets logger to handle possible output from service binding process.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets a method for additional <see cref="ServerServiceDefinition"/> configuration.
        /// </summary>
        public Func<ServerServiceDefinition, ServerServiceDefinition> ConfigureServiceDefinition { get; set; }
    }
}
