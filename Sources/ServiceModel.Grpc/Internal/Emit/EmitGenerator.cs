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
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitGenerator : IGenerator
    {
        public ILogger? Logger { get; set; }

        public IClientBuilder<TContract> GenerateClientBuilder<TContract>()
        {
            var serviceType = typeof(TContract);

            Type clientBuilderType;
            lock (ProxyAssembly.SyncRoot)
            {
                clientBuilderType = ProxyAssembly.DefaultModule.GetType(ContractDescription.GetClientBuilderClassName(serviceType), false, false)!;
                if (clientBuilderType == null)
                {
                    var (description, contractType) = GenerateContract(serviceType);
                    var clientType = new EmitClientBuilder(description, contractType).Build(ProxyAssembly.DefaultModule);
                    clientBuilderType = new EmitClientBuilderBuilder(description, contractType, clientType).Build(ProxyAssembly.DefaultModule);
                }
            }

            return (IClientBuilder<TContract>)Activator.CreateInstance(clientBuilderType);
        }

        public void BindService(IServiceBinder binder, Type serviceType, IMarshallerFactory marshallerFactory)
        {
            if (!ServiceContract.IsServiceInstanceType(serviceType))
            {
                throw new ArgumentOutOfRangeException(nameof(serviceType));
            }

            if (ServiceContract.IsNativeGrpcService(serviceType))
            {
                Logger?.LogDebug("Ignore service {0} binding: native grpc service.", serviceType.FullName);
                return;
            }

            ContractDescription description;
            Type contractType;
            Type channelType;
            lock (ProxyAssembly.SyncRoot)
            {
                (description, contractType) = GenerateContract(serviceType);
                channelType = ProxyAssembly.DefaultModule.GetType(ContractDescription.GetServiceClassName(serviceType), false, false)!;
                if (channelType == null)
                {
                    channelType = GenerateChannel(description, serviceType, contractType);
                }
            }

            var contract = EmitContractBuilder.CreateFactory(contractType)(marshallerFactory);
            var channelInstance = EmitServiceBuilder.CreateFactory(channelType, contractType)(contract);

            var addMethodGeneric = typeof(EmitGenerator).StaticMethod(nameof(ServiceBinderAddMethod));
            foreach (var interfaceDescription in description.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    var message = operation.Message;

                    var addMethod = addMethodGeneric
                        .MakeGenericMethod(message.RequestType, message.ResponseType)
                        .CreateDelegate<Action<IServiceBinder, object, ServiceCallInfo>>();

                    var callInfo = new ServiceCallInfo(
                        ReflectionTools.ImplementationOfMethod(serviceType, interfaceDescription.InterfaceType, message.Operation),
                        channelType.InstanceMethod(operation.OperationName),
                        channelInstance);

                    var grpcMethodMethod = contractType.InstanceFiled(operation.GrpcMethodName).GetValue(contract);

                    Logger?.LogDebug("Bind service method {0}.{1}.", serviceType.FullName, callInfo.ServiceInstanceMethod.Name);
                    addMethod(binder, grpcMethodMethod, callInfo);
                }
            }
        }

        private static ContractDescription CreateDescription(Type serviceType, ILogger? logger)
        {
            var contractDescription = new ContractDescription(serviceType);

            foreach (var interfaceDescription in contractDescription.Interfaces)
            {
                logger?.LogDebug("{0}: {1} is not service contract.", serviceType.FullName, interfaceDescription.InterfaceType.FullName);
            }

            foreach (var interfaceDescription in contractDescription.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    logger?.LogDebug("{0}: {1}", serviceType.FullName, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    logger?.LogError("{0}: {1}", serviceType.FullName, method.Error);
                }
            }

            return contractDescription;
        }

        private static void ServiceBinderAddMethod<TRequest, TResponse>(
            IServiceBinder binder,
            object grpcMethodMethod,
            ServiceCallInfo callInfo)
            where TRequest : class
            where TResponse : class
        {
            var method = (Method<TRequest, TResponse>)grpcMethodMethod;

            switch (method.Type)
            {
                case MethodType.Unary:
                    binder.AddUnaryServerMethod(method, callInfo);
                    return;

                case MethodType.ClientStreaming:
                    binder.AddClientStreamingServerMethod(method, callInfo);
                    return;

                case MethodType.ServerStreaming:
                    binder.AddServerStreamingServerMethod(method, callInfo);
                    return;

                case MethodType.DuplexStreaming:
                    binder.AddDuplexStreamingServerMethod(method, callInfo);
                    return;
            }

            throw new NotImplementedException("{0} operation is not implemented.".FormatWith(method.Type));
        }

        private Type GenerateChannel(ContractDescription description, Type serviceType, Type contractType)
        {
            var serviceBuilder = new EmitServiceBuilder(ProxyAssembly.DefaultModule, description.ServiceClassName, contractType);

            foreach (var interfaceDescription in description.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    if (EmitServiceBuilder.IsSupportedContextInput(operation.Message))
                    {
                        serviceBuilder.BuildOperation(operation, interfaceDescription.InterfaceType);
                    }
                    else
                    {
                        var error = "Context options in [{0}] are not supported.".FormatWith(ReflectionTools.GetSignature(operation.Message.Operation));
                        Logger?.LogError("Service {0}: {1}", serviceType.FullName, error);
                        serviceBuilder.BuildNotSupportedOperation(operation, interfaceDescription.InterfaceType, error);
                    }
                }
            }

            return serviceBuilder.BuildType();
        }

        private (ContractDescription Description, Type ContractType) GenerateContract(Type serviceType)
        {
            var className = ContractDescription.GetContractClassName(serviceType);
            var contractType = ProxyAssembly.DefaultModule.GetType(className, false, false);

            ContractDescription description;
            if (contractType == null)
            {
                description = CreateDescription(serviceType, Logger);
                contractType = new EmitContractBuilder(description).Build(ProxyAssembly.DefaultModule);
            }
            else
            {
                description = CreateDescription(serviceType, null);
            }

            return (description, contractType);
        }
    }
}
