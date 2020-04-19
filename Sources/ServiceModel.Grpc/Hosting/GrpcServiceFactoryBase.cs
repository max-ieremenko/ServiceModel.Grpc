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
            if (ServiceContract.IsNativeGrpcService(_serviceType))
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
            var contractDescription = new ContractDescription(_serviceType);

            foreach (var interfaceDescription in contractDescription.Interfaces)
            {
                Logger.LogDebug("{0}: {1} is not service contract.", _serviceType.FullName, interfaceDescription.InterfaceType.FullName);
            }

            foreach (var interfaceDescription in contractDescription.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    Logger.LogDebug("{0}: {1}", _serviceType.FullName, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    Logger.LogError("{0}: {1}", _serviceType.FullName, method.Error);
                }

                if (interfaceDescription.Operations.Count > 0)
                {
                    Type serviceChannelType;
                    lock (ProxyAssembly.SyncRoot)
                    {
                        serviceChannelType = BuildChannelService(interfaceDescription.InterfaceType, interfaceDescription.Operations);
                    }

                    BindInterface(serviceChannelType, interfaceDescription.InterfaceType, interfaceDescription.Operations);
                }
            }
        }

        private Type BuildChannelService(Type interfaceType, IList<OperationDescription> operations)
        {
            var serviceBuilder = new GrpcServiceBuilder(interfaceType, _marshallerFactory);
            foreach (var operation in operations)
            {
                var message = operation.Message;

                if (message.ContextInput.Any(i => !ServerChannelAdapter.TryGetServiceContextOptionMethod(message.Parameters[i].ParameterType)))
                {
                    var error = "Context options in [{0}] are not supported.".FormatWith(ReflectionTools.GetSignature(message.Operation));

                    Logger.LogError("Service {0}: {1}", _serviceType.FullName, error);
                    serviceBuilder.BuildNotSupportedCall(message, operation.OperationName, error);
                }
                else
                {
                    serviceBuilder.BuildCall(message, operation.OperationName);
                }
            }

            return serviceBuilder.BuildType();
        }

        private void BindInterface(Type serviceChannelType, Type interfaceType, IList<OperationDescription> operations)
        {
            var addMethodGeneric = typeof(GrpcServiceFactoryBase<TService>).InstanceMethod(nameof(AddMethod));
            foreach (var operation in operations)
            {
                var message = operation.Message;

                var addMethod = addMethodGeneric
                    .MakeGenericMethod(message.RequestType, message.ResponseType)
                    .CreateDelegate<Action<string, string, MethodType, ServiceCallInfo>>(this);

                var callInfo = new ServiceCallInfo(
                    ReflectionTools.ImplementationOfMethod(_serviceType, interfaceType, message.Operation),
                    serviceChannelType.StaticMethod(operation.OperationName));

                Logger.LogDebug("Bind method {0}.{1}.", _serviceType.FullName, callInfo.ServiceInstanceMethod.Name);
                addMethod(operation.ServiceName, operation.OperationName, message.OperationType, callInfo);
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
