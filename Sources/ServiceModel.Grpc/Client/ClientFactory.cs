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
    public sealed class ClientFactory : IClientFactory
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

        internal IServiceClientBuilder CreateClientBuilder(ServiceModelGrpcClientOptions clientOptions)
        {
            var builder = clientOptions.ClientBuilder?.Invoke() ?? new GrpcServiceClientBuilder();

            builder.MarshallerFactory = clientOptions.MarshallerFactory ?? DataContractMarshallerFactory.Default;
            builder.DefaultCallOptions = clientOptions.DefaultCallOptions;
            builder.Logger = clientOptions.Logger;

            return builder;
        }

        private Delegate RegisterClient<TContract>(Action<ServiceModelGrpcClientOptions> configure)
            where TContract : class
        {
            var contractType = typeof(TContract);

            if (!ReflectionTools.IsPublicInterface(contractType))
            {
                throw new NotSupportedException("{0} is not supported. Client contract must be public interface.".FormatWith(contractType));
            }

            var options = new ServiceModelGrpcClientOptions
            {
                MarshallerFactory = _defaultOptions?.MarshallerFactory,
                DefaultCallOptions = _defaultOptions?.DefaultCallOptions,
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
