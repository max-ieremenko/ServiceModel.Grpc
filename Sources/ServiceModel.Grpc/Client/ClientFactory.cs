using System;
using System.Collections.Generic;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Client
{
    public sealed class ClientFactory
    {
        private readonly ServiceModelGrpcClientOptions _defaultOptions;
        private readonly IDictionary<Type, Delegate> _factoryByContract;

        public ClientFactory(ServiceModelGrpcClientOptions defaultOptions = null)
        {
            _defaultOptions = defaultOptions;
            _factoryByContract = new Dictionary<Type, Delegate>();
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

            if (!_factoryByContract.TryGetValue(typeof(TContract), out var factory))
            {
                factory = RegisterClient<TContract>(null);
            }

            var method = (Func<CallInvoker, TContract>)factory;
            return method(callInvoker);
        }

        public Delegate RegisterClient<TContract>(Action<ServiceModelGrpcClientOptions> configure)
            where TContract : class
        {
            ServiceModelGrpcClientOptions options = null;
            if (configure != null)
            {
                options = new ServiceModelGrpcClientOptions();
                configure(options);
            }

            var marshallerFactory = (options?.MarshallerFactory ?? _defaultOptions?.MarshallerFactory) ?? DataContractMarshallerFactory.Default;
            var factory = new GrpcServiceClientBuilder<TContract>().Build(marshallerFactory);
            _factoryByContract.Add(typeof(TContract), factory);
            return factory;
        }
    }
}
