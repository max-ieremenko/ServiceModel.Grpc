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
using System.Globalization;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
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
        private static int _instanceCounter;

        private readonly object _syncRoot;
        private readonly ServiceModelGrpcClientOptions? _defaultOptions;
        private readonly IDictionary<Type, Delegate> _factoryByContract;
        private readonly IDictionary<Type, Interceptor> _interceptorByContract;
        private readonly string _factoryId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientFactory"/> class.
        /// </summary>
        /// <param name="defaultOptions">Default configuration for all clients, created by this instance.</param>
        public ClientFactory(ServiceModelGrpcClientOptions? defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
            _factoryByContract = new Dictionary<Type, Delegate>();
            _interceptorByContract = new Dictionary<Type, Interceptor>();
            _syncRoot = new object();

            var instanceNumber = Interlocked.Increment(ref _instanceCounter);
            _factoryId = instanceNumber.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Configures a proxy for gRPC service contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">The service contract type.</typeparam>
        /// <param name="configure">The configuration action.</param>
        public void AddClient<TContract>(Action<ServiceModelGrpcClientOptions>? configure = null)
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
            Interceptor interceptor;
            lock (_syncRoot)
            {
                var contractType = typeof(TContract);
                if (!_factoryByContract.TryGetValue(contractType, out factory))
                {
                    factory = RegisterClient<TContract>(null);
                }

                _interceptorByContract.TryGetValue(contractType, out interceptor);
            }

            var method = (Func<CallInvoker, TContract>)factory;
            if (interceptor != null)
            {
                callInvoker = callInvoker.Intercept(interceptor);
            }

            return method(callInvoker);
        }

        internal IServiceClientBuilder CreateClientBuilder(ServiceModelGrpcClientOptions clientOptions)
        {
            var builder = clientOptions.ClientBuilder?.Invoke() ?? new GrpcServiceClientBuilder();

            builder.MarshallerFactory = clientOptions.MarshallerFactory.ThisOrDefault();
            builder.DefaultCallOptionsFactory = clientOptions.DefaultCallOptionsFactory;
            builder.Logger = clientOptions.Logger;

            return builder;
        }

        private Delegate RegisterClient<TContract>(Action<ServiceModelGrpcClientOptions>? configure)
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
                ErrorHandler = _defaultOptions?.ErrorHandler,
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

                if (options.ErrorHandler != null)
                {
                    _interceptorByContract.Add(contractType, new ClientNativeInterceptor(new ClientCallErrorInterceptor(
                        options.ErrorHandler,
                        options.MarshallerFactory.ThisOrDefault(),
                        options.Logger)));
                }
            }

            return factory;
        }
    }
}
