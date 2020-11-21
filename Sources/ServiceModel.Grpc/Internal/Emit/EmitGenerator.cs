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
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitGenerator : IEmitGenerator
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

        public IServiceEndpointBinder<TService> GenerateServiceEndpointBinder<TService>(Type? serviceInstanceType)
        {
            if (serviceInstanceType != null && !ServiceContract.IsServiceInstanceType(serviceInstanceType))
            {
                throw new ArgumentOutOfRangeException(nameof(serviceInstanceType));
            }

            var serviceType = typeof(TService);

            ContractDescription description;
            Type contractType;
            Type channelType;
            lock (ProxyAssembly.SyncRoot)
            {
                (description, contractType) = GenerateContract(serviceType);
                channelType = ProxyAssembly.DefaultModule.GetType(ContractDescription.GetEndpointClassName(serviceType), false, false)!;
                if (channelType == null)
                {
                    var serviceBuilder = new EmitServiceEndpointBuilder(description, contractType);
                    channelType = serviceBuilder.Build(ProxyAssembly.DefaultModule, Logger);
                }
            }

            return new EmitServiceEndpointBinder<TService>(description, serviceInstanceType, contractType, channelType, Logger);
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
