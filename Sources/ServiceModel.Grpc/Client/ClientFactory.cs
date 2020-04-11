using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Client
{
    public sealed class ClientFactory
    {
        private static int _instanceCounter;

        private readonly object _syncRoot;
        private readonly ServiceModelGrpcClientOptions _defaultOptions;
        private readonly IDictionary<Type, Delegate> _factoryByContract;
        private readonly string _factoryId;

        public ClientFactory(ServiceModelGrpcClientOptions defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
            _factoryByContract = new Dictionary<Type, Delegate>();
            _syncRoot = new object();

            var instanceNumber = Interlocked.Increment(ref _instanceCounter);
            _factoryId = instanceNumber.ToString(CultureInfo.InvariantCulture);
        }

        public void AddClient<TContract>(Action<ServiceModelGrpcClientOptions> configure = null)
            where TContract : class
        {
            RegisterClient<TContract>(configure);
        }

        public TContract CreateClient<TContract>(ChannelBase channel)
            where TContract : class
        {
            channel.AssertNotNull(nameof(channel));

            return CreateClient<TContract>(channel.CreateCallInvoker());
        }

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

        public Delegate RegisterClient<TContract>(Action<ServiceModelGrpcClientOptions> configure)
            where TContract : class
        {
            var options = new ServiceModelGrpcClientOptions();
            configure?.Invoke(options);

            var builder = CreateClientBuilder(options);
            var contractType = typeof(TContract);

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

        internal IServiceClientBuilder CreateClientBuilder(ServiceModelGrpcClientOptions clientOptions)
        {
            var builderFactory = clientOptions.ClientBuilder ?? _defaultOptions?.ClientBuilder;
            var builder = builderFactory?.Invoke() ?? new GrpcServiceClientBuilder();

            builder.MarshallerFactory = (clientOptions.MarshallerFactory ?? _defaultOptions?.MarshallerFactory) ?? DataContractMarshallerFactory.Default;
            builder.DefaultCallOptions = clientOptions.DefaultCallOptions ?? _defaultOptions?.DefaultCallOptions;
            builder.Logger = clientOptions.Logger ?? _defaultOptions?.Logger;

            return builder;
        }
    }
}
