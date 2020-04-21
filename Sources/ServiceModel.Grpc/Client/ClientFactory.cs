using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Client
{
    /// <summary>
    /// Serves to configure and create instances of gRPC service clients.
    /// </summary>
    public sealed class ClientFactory : IClientFactory
    {
        private static int _instanceCounter;

        private readonly object _syncRoot;
        private readonly ServiceModelGrpcClientOptions _defaultOptions;
        private readonly IDictionary<Type, Delegate> _factoryByContract;
        private readonly string _factoryId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientFactory"/> class.
        /// </summary>
        /// <param name="defaultOptions">Default configuration for all clients, created by this instance.</param>
        public ClientFactory(ServiceModelGrpcClientOptions defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
            _factoryByContract = new Dictionary<Type, Delegate>();
            _syncRoot = new object();

            var instanceNumber = Interlocked.Increment(ref _instanceCounter);
            _factoryId = instanceNumber.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Configures a proxy for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="configure">The configuration action.</param>
        public void AddClient<TContract>(Action<ServiceModelGrpcClientOptions> configure = null)
            where TContract : class
        {
            RegisterClient<TContract>(configure);
        }

        /// <summary>
        /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="channel">The gRPC channel.</param>
        /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
        public TContract CreateClient<TContract>(ChannelBase channel)
            where TContract : class
        {
            channel.AssertNotNull(nameof(channel));

            return CreateClient<TContract>(channel.CreateCallInvoker());
        }

        /// <summary>
        /// Creates a new proxy instance for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="callInvoker">The client-side RPC invocation.</param>
        /// <returns>The proxy for <typeparamref name="TContract"/>.</returns>
        public TContract CreateClient<TContract>(CallInvoker callInvoker)
            where TContract : class
        {
            callInvoker.AssertNotNull(nameof(callInvoker));

            Delegate factory;
            lock (_syncRoot)
            {
                if (!_factoryByContract.TryGetValue(typeof(TContract), out factory))
                {
                    factory = RegisterClient<TContract>(null);
                }
            }

            var method = (Func<CallInvoker, TContract>)factory;
            return method(callInvoker);
        }

        internal IServiceClientBuilder CreateClientBuilder(ServiceModelGrpcClientOptions clientOptions)
        {
            var builder = clientOptions.ClientBuilder?.Invoke() ?? new GrpcServiceClientBuilder();

            builder.MarshallerFactory = clientOptions.MarshallerFactory ?? DataContractMarshallerFactory.Default;
            builder.DefaultCallOptionsFactory = clientOptions.DefaultCallOptionsFactory;
            builder.Logger = clientOptions.Logger;

            return builder;
        }

        private Delegate RegisterClient<TContract>(Action<ServiceModelGrpcClientOptions> configure)
            where TContract : class
        {
            var contractType = typeof(TContract);

            if (!ReflectionTools.IsPublicInterface(contractType) || contractType.IsGenericTypeDefinition)
            {
                throw new NotSupportedException("{0} is not supported. Client contract must be public non-generic interface.".FormatWith(contractType));
            }

            var options = new ServiceModelGrpcClientOptions
            {
                MarshallerFactory = _defaultOptions?.MarshallerFactory,
                DefaultCallOptionsFactory = _defaultOptions?.DefaultCallOptionsFactory,
                Logger = _defaultOptions?.Logger,
                ClientBuilder = _defaultOptions?.ClientBuilder
            };

            configure?.Invoke(options);

            var builder = CreateClientBuilder(options);

            Func<CallInvoker, TContract> factory;
            lock (_syncRoot)
            {
                if (_factoryByContract.ContainsKey(contractType))
                {
                    throw new InvalidOperationException("Client for contract {0} is already initialized and cannot be changed.".FormatWith(contractType.FullName));
                }

                factory = builder.Build<TContract>(_factoryId);
                _factoryByContract.Add(contractType, factory);
            }

            return factory;
        }
    }
}
