using System;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Client
{
    /// <summary>
    /// Provides configuration used by <see cref="IClientFactory"/>.
    /// </summary>
    public sealed class ServiceModelGrpcClientOptions
    {
        /// <summary>
        /// Gets or sets a factory for serializing and deserializing messages.
        /// </summary>
        public IMarshallerFactory MarshallerFactory { get; set; }

        /// <summary>
        /// Gets or sets a methods which provides <see cref="CallOptions"/> for all calls made by all clients created by <see cref="IClientFactory"/>.
        /// </summary>
        public Func<CallOptions> DefaultCallOptionsFactory { get; set; }

        /// <summary>
        /// Gets or sets logger to handle possible output from <see cref="IClientFactory"/>.
        /// </summary>
        public ILogger Logger { get; set; }

        internal Func<IServiceClientBuilder> ClientBuilder { get; set; }
    }
}
