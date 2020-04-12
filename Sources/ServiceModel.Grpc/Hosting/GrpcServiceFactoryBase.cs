using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Hosting
{
    internal abstract class GrpcServiceFactoryBase<TService>
    {
        private readonly IMarshallerFactory _marshallerFactory;
        private readonly Type _serviceType;

        protected GrpcServiceFactoryBase(ILogger logger, IMarshallerFactory marshallerFactory)
        {
            logger.AssertNotNull(nameof(logger));

            Logger = logger;
            _marshallerFactory = marshallerFactory ?? DataContractMarshallerFactory.Default;
            _serviceType = typeof(TService);
        }

        public ILogger Logger { get; }

        public void Bind()
        {
            if (ContractDescription.IgnoreServiceBinding(_serviceType))
            {
                Logger.LogDebug("Ignore service {0} binding: native grpc service.", _serviceType.FullName);
            }
            else
            {
                BindCore();
            }
        }

        protected abstract void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class;

        private void BindCore()
        {
            foreach (var interfaceType in ContractDescription.GetInterfacesImplementation(_serviceType))
            {
                var messages = new List<MessageAssembler>();

                foreach (var method in ContractDescription.GetMethodsForImplementation(interfaceType))
                {
                    if (!ContractDescription.IsOperationMethod(interfaceType, method))
                    {
                        Logger.LogDebug("Skip {0} method {1}.{2}: is not operation contract.", _serviceType.FullName, interfaceType.FullName, method.Name);
                        continue;
                    }

                    if (ContractDescription.TryCreateMessage(method, out var message, out var error))
                    {
                        messages.Add(message);
                    }
                    else
                    {
                        Logger.LogError("Service {0}: {1}", _serviceType.FullName, error);
                    }
                }

                if (messages.Count > 0)
                {
                    Type serviceChannelType;
                    lock (ProxyAssembly.SyncRoot)
                    {
                        serviceChannelType = BuildChannelService(interfaceType, messages);
                    }

                    BindInterface(serviceChannelType, interfaceType, messages);
                }
            }
        }

        private Type BuildChannelService(Type interfaceType, IList<MessageAssembler> messages)
        {
            var serviceBuilder = new GrpcServiceBuilder(interfaceType);
            foreach (var message in messages)
            {
                if (message.ContextInput.Any(i => ContractDescription.GetServiceContextOption(message.Parameters[i].ParameterType) == null))
                {
                    var error = "Context options in [{0}] are not supported.".FormatWith(ReflectionTools.GetSignature(message.Operation));

                    Logger.LogError("Service {0}: {1}", _serviceType.FullName, error);
                    serviceBuilder.BuildNotSupportedCall(message, error);
                }
                else
                {
                    serviceBuilder.BuildCall(message);
                }
            }

            return serviceBuilder.BuildType();
        }

        private void BindInterface(Type serviceChannelType, Type interfaceType, IList<MessageAssembler> messages)
        {
            var serviceName = ContractDescription.GetServiceName(interfaceType);
            var addMethodGeneric = typeof(GrpcServiceFactoryBase<TService>).InstanceMethod(nameof(AddMethod));
            foreach (var message in messages)
            {
                var addMethod = addMethodGeneric
                    .MakeGenericMethod(message.RequestType, message.ResponseType)
                    .CreateDelegate<Action<string, string, MethodType, ServiceCallInfo>>(this);

                var operationName = ContractDescription.GetOperationName(message.Operation);
                var callInfo = new ServiceCallInfo(
                    ReflectionTools.ImplementationOfMethod(_serviceType, interfaceType, message.Operation),
                    serviceChannelType.StaticMethod(operationName));

                Logger.LogDebug("Bind method {0}.{1}.", _serviceType.FullName, callInfo.ServiceInstanceMethod.Name);
                addMethod(serviceName, operationName, message.OperationType, callInfo);
            }
        }

        private void AddMethod<TRequest, TResponse>(
            string serviceName,
            string operationName,
            MethodType operationType,
            ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var method = new Method<TRequest, TResponse>(
                operationType,
                serviceName,
                operationName,
                _marshallerFactory.CreateMarshaller<TRequest>(),
                _marshallerFactory.CreateMarshaller<TResponse>());
            
            switch (operationType)
            {
                case MethodType.Unary:
                    AddUnaryServerMethod(method, callInfo);
                    return;

                case MethodType.ClientStreaming:
                    AddClientStreamingServerMethod(method, callInfo);
                    return;

                case MethodType.ServerStreaming:
                    AddServerStreamingServerMethod(method, callInfo);
                    return;

                case MethodType.DuplexStreaming:
                    AddDuplexStreamingServerMethod(method, callInfo);
                    return;
            }

            throw new NotImplementedException("{0} operation is not implemented.".FormatWith(operationType));
        }
    }
}
