using System;
using System.Collections.Generic;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class ServiceModelServiceMethodProvider<TService> : IServiceMethodProvider<TService>
        where TService : class
    {
        private readonly ServiceModelGrpcServiceOptions _rootConfiguration;
        private readonly ServiceModelGrpcServiceOptions<TService> _serviceConfiguration;

        public ServiceModelServiceMethodProvider(
            IOptions<ServiceModelGrpcServiceOptions> rootConfiguration,
            IOptions<ServiceModelGrpcServiceOptions<TService>> serviceConfiguration)
        {
            rootConfiguration.AssertNotNull(nameof(rootConfiguration));
            serviceConfiguration.AssertNotNull(nameof(serviceConfiguration));

            _rootConfiguration = rootConfiguration.Value;
            _serviceConfiguration = serviceConfiguration.Value;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var serviceContracts = ServiceContract.GetServiceContractInterfaces(typeof(TService));
            if (serviceContracts.Count == 0)
            {
                throw new InvalidOperationException("The service [{0}] does not implement any public interfaces marked as [ServiceContract].".FormatWith(typeof(TService)));
            }

            var addMethodGeneric = GetType().InstanceMethod(nameof(AddMethod));

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
                    var addMethod = (Action<ServiceMethodProviderContext<TService>, string, string, MethodType, GrpcServiceBuilder>)addMethodGeneric
                        .MakeGenericMethod(message.RequestType, message.ResponseType)
                        .CreateDelegate(typeof(Action<ServiceMethodProviderContext<TService>, string, string, MethodType, GrpcServiceBuilder>), this);

                    addMethod(
                        context,
                        serviceName,
                        ServiceContract.GetServiceOperationName(message.Operation),
                        message.OperationType,
                        serviceBuilder);
                }
            }
        }

        private void AddMethod<TRequest, TResponse>(
            ServiceMethodProviderContext<TService> context,
            string serviceName,
            string operationName,
            MethodType operationType,
            GrpcServiceBuilder serviceBuilder)
            where TRequest : class
            where TResponse : class
        {
            var marshallerFactory = (_serviceConfiguration.MarshallerFactory ?? _rootConfiguration.DefaultMarshallerFactory) ?? DataContractMarshallerFactory.Default;

            var method = new Method<TRequest, TResponse>(
                operationType,
                serviceName,
                operationName,
                marshallerFactory.CreateMarshaller<TRequest>(),
                marshallerFactory.CreateMarshaller<TResponse>());

            switch (operationType)
            {
                case MethodType.Unary:
                {
                    var invoker = serviceBuilder.CreateCall<UnaryServerMethod<TService, TRequest, TResponse>>(method.Name);
                    context.AddUnaryMethod(method, Array.Empty<object>(), invoker);
                    return;
                }

                case MethodType.ClientStreaming:
                {
                    var invoker = serviceBuilder.CreateCall<ClientStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
                    context.AddClientStreamingMethod(method, Array.Empty<object>(), invoker);
                    return;
                }

                case MethodType.ServerStreaming:
                {
                    var invoker = serviceBuilder.CreateCall<ServerStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
                    context.AddServerStreamingMethod(method, Array.Empty<object>(), invoker);
                    return;
                }

                case MethodType.DuplexStreaming:
                {
                    var invoker = serviceBuilder.CreateCall<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
                    context.AddDuplexStreamingMethod(method, Array.Empty<object>(), invoker);
                    return;
                }
            }

            throw new NotImplementedException("{0} operation is not implemented.".FormatWith(operationType));
        }
    }
}
