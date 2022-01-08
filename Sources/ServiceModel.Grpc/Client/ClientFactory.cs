// <copyright>
// Copyright 2020 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors.Internal;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Client
{
    /// <summary>
    /// Serves to configure and create instances of gRPC service clients.
    /// </summary>
    public sealed class ClientFactory : IClientFactory
    {
        private readonly object _syncRoot;
        private readonly IEmitGenerator? _generator;
        private readonly ServiceModelGrpcClientOptions? _defaultOptions;
        private readonly IDictionary<Type, object> _builderByContract;
        private readonly IDictionary<Type, Interceptor> _interceptorByContract;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientFactory"/> class.
        /// </summary>
        /// <param name="defaultOptions">Default configuration for all clients, created by this instance.</param>
        public ClientFactory(ServiceModelGrpcClientOptions? defaultOptions = null)
            : this(null, defaultOptions)
        {
        }

        internal ClientFactory(IEmitGenerator? generator, ServiceModelGrpcClientOptions? defaultOptions)
        {
            _generator = generator;
            _defaultOptions = defaultOptions;
            _builderByContract = new Dictionary<Type, object>();
            _interceptorByContract = new Dictionary<Type, Interceptor>();
            _syncRoot = new object();
        }

        /// <summary>
        /// Configures the factory to generate a proxy automatically for gRPC service contract <typeparamref name="TContract"/> with specific options.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="configure">The action to configure options.</param>
        public void AddClient<TContract>(Action<ServiceModelGrpcClientOptions>? configure = null)
            where TContract : class
        {
            RegisterClient<TContract>(null, configure);
        }

        /// <summary>
        /// Configures the factory to use a builder for proxy creation with specific options.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="builder">The proxy builder.</param>
        /// <param name="configure">The action to configure options.</param>
        public void AddClient<TContract>(IClientBuilder<TContract> builder, Action<ServiceModelGrpcClientOptions>? configure = null)
            where TContract : class
        {
            builder.AssertNotNull(nameof(builder));

            RegisterClient(builder, configure);
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

            object factory;
            Interceptor interceptor;
            lock (_syncRoot)
            {
                var contractType = typeof(TContract);
                if (!_builderByContract.TryGetValue(contractType, out factory))
                {
                    factory = RegisterClient<TContract>(null, null);
                }

                _interceptorByContract.TryGetValue(contractType, out interceptor);
            }

            var builder = (IClientBuilder<TContract>)factory;
            if (interceptor != null)
            {
                callInvoker = callInvoker.Intercept(interceptor);
            }

            return builder.Build(callInvoker);
        }

        private IEmitGenerator CreateGenerator(ServiceModelGrpcClientOptions clientOptions)
        {
            var generator = _generator ?? new EmitGenerator();
            generator.Logger = clientOptions.Logger;

            return generator;
        }

        private object RegisterClient<TContract>(IClientBuilder<TContract>? userBuilder, Action<ServiceModelGrpcClientOptions>? configure)
            where TContract : class
        {
            var contractType = typeof(TContract);

            if (userBuilder == null && (!ReflectionTools.IsPublicInterface(contractType) || contractType.IsGenericTypeDefinition))
            {
                throw new NotSupportedException("{0} is not supported. Client contract must be public non-generic interface.".FormatWith(contractType));
            }

            var options = new ServiceModelGrpcClientOptions
            {
                MarshallerFactory = _defaultOptions?.MarshallerFactory,
                DefaultCallOptionsFactory = _defaultOptions?.DefaultCallOptionsFactory,
                Logger = _defaultOptions?.Logger,
                ErrorHandler = _defaultOptions?.ErrorHandler
            };

            configure?.Invoke(options);

            var generator = userBuilder == null ? CreateGenerator(options) : null;

            IClientBuilder<TContract> builder;
            lock (_syncRoot)
            {
                if (_builderByContract.ContainsKey(contractType))
                {
                    throw new InvalidOperationException("Client for contract {0} is already initialized and cannot be changed.".FormatWith(contractType.FullName));
                }

                builder = userBuilder ?? generator!.GenerateClientBuilder<TContract>();
                builder.Initialize(options.MarshallerFactory ?? DataContractMarshallerFactory.Default, options.DefaultCallOptionsFactory);

                _builderByContract.Add(contractType, builder);

                if (options.ErrorHandler != null)
                {
                    _interceptorByContract.Add(contractType, new ClientNativeInterceptor(new ClientCallErrorInterceptor(
                        options.ErrorHandler,
                        options.MarshallerFactory.ThisOrDefault(),
                        options.Logger)));
                }
            }

            return builder;
        }
    }
}
