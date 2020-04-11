using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Hosting
{
    internal abstract class GrpcServiceFactoryBase<TService>
    {
        private readonly IMarshallerFactory _marshallerFactory;

        protected GrpcServiceFactoryBase(ILog logger, IMarshallerFactory marshallerFactory)
        {
            logger.AssertNotNull(nameof(logger));

            Logger = logger;
            _marshallerFactory = marshallerFactory ?? DataContractMarshallerFactory.Default;
        }

        public ILog Logger { get; }

        public void Bind()
        {
            var serviceType = typeof(TService);

            var serviceContracts = ServiceContract.GetServiceContractInterfaces(serviceType);
            if (serviceContracts.Count == 0)
            {
                Logger.LogError("The [{0}] does not implement any contracts.".FormatWith(serviceType));
                return;
            }

            var addMethodGeneric = typeof(GrpcServiceFactoryBase<TService>).InstanceMethod(nameof(AddMethod));

            foreach (var serviceContract in serviceContracts)
            {
                var serviceName = ServiceContract.GetServiceName(serviceContract);
                var operations = ServiceContract.GetServiceOperations(serviceContract);

                var serviceBuilder = new GrpcServiceBuilder(serviceContract);
                var messages = new List<MessageAssembler>();

                foreach (var operation in operations)
                {
                    var message = new MessageAssembler(operation);
                    serviceBuilder.BuildCall(message);
                    messages.Add(message);
                }

                foreach (var message in messages)
                {
                    var addMethod = (Action<string, string, MethodType, MethodInfo, GrpcServiceBuilder>)addMethodGeneric
                        .MakeGenericMethod(message.RequestType, message.ResponseType)
                        .CreateDelegate(typeof(Action<string, string, MethodType, MethodInfo, GrpcServiceBuilder>), this);

                    var serviceMethod = ReflectionTools.ImplementationOfMethod(serviceType, serviceContract, message.Operation);

                    addMethod(
                        serviceName,
                        ServiceContract.GetServiceOperationName(message.Operation),
                        message.OperationType,
                        serviceMethod,
                        serviceBuilder);
                }
            }
        }

        protected abstract void AddUnaryServerMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            MethodInfo serviceMethod,
            GrpcServiceBuilder serviceBuilder)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddClientStreamingServerMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            MethodInfo serviceMethod,
            GrpcServiceBuilder serviceBuilder)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddServerStreamingServerMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            MethodInfo serviceMethod,
            GrpcServiceBuilder serviceBuilder)
            where TRequest : class
            where TResponse : class;

        protected abstract void AddDuplexStreamingServerMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            MethodInfo serviceMethod,
            GrpcServiceBuilder serviceBuilder)
            where TRequest : class
            where TResponse : class;

        private void AddMethod<TRequest, TResponse>(
            string serviceName,
            string operationName,
            MethodType operationType,
            MethodInfo serviceMethod,
            GrpcServiceBuilder serviceBuilder)
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
                    AddUnaryServerMethod(method, serviceMethod, serviceBuilder);
                    return;

                case MethodType.ClientStreaming:
                    AddClientStreamingServerMethod(method, serviceMethod, serviceBuilder);
                    return;

                case MethodType.ServerStreaming:
                    AddServerStreamingServerMethod(method, serviceMethod, serviceBuilder);
                    return;

                case MethodType.DuplexStreaming:
                    AddDuplexStreamingServerMethod(method, serviceMethod, serviceBuilder);
                    return;
            }

            throw new NotImplementedException("{0} operation is not implemented.".FormatWith(operationType));
        }
    }
}
