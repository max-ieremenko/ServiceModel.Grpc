using System;
using Grpc.Core;

namespace ServiceModel.Grpc.Client
{
    /// <summary>
    /// Represents a type used to configure and create instances of gRPC service clients.
    /// </summary>
    public interface IClientFactory
    {
        /// <summary>
        /// Configures a proxy for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="configure">The configuration action.</param>
        void AddClient<TContract>(Action<ServiceModelGrpcClientOptions> configure = null)
            where TContract : class;

        /// <summary>
        /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="channel">The gRPC channel.</param>
        /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
        TContract CreateClient<TContract>(ChannelBase channel)
            where TContract : class;

        /// <summary>
        /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="callInvoker">The client-side RPC invocation.</param>
        /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
        TContract CreateClient<TContract>(CallInvoker callInvoker)
            where TContract : class;
    }
}
