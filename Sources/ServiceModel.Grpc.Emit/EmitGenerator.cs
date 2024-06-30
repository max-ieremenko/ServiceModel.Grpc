// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Reflection;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.CodeGenerators;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit;

public static class EmitGenerator
{
    public static IClientBuilder<TContract> GenerateClientBuilder<TContract>(ILogger? logger)
    {
        var serviceType = typeof(TContract);

        Type? clientBuilderType;
        lock (ProxyAssembly.SyncRoot)
        {
            clientBuilderType = ProxyAssembly.DefaultModule.GetType(ContractDescriptionBuilder.GetClientBuilderClassName(serviceType), false, false);
            if (clientBuilderType == null)
            {
                var (description, contractType) = GenerateContract(serviceType, logger);
                var clientType = new EmitClientBuilder(description, contractType).Build(ProxyAssembly.DefaultModule);
                clientBuilderType = new EmitClientBuilderBuilder(description, contractType, clientType).Build(ProxyAssembly.DefaultModule);
            }
        }

        return (IClientBuilder<TContract>)Activator.CreateInstance(clientBuilderType);
    }

    public static IServiceEndpointBinder<TService> GenerateServiceEndpointBinder<TService>(Type? serviceInstanceType, ILogger? logger)
    {
        var serviceType = typeof(TService);

        ContractDescription<Type> description;
        Type contractType;
        Type? channelType;
        lock (ProxyAssembly.SyncRoot)
        {
            (description, contractType) = GenerateContract(serviceType, logger);
            channelType = ProxyAssembly.DefaultModule.GetType(ContractDescriptionBuilder.GetEndpointClassName(serviceType), false, false);
            if (channelType == null)
            {
                var serviceBuilder = new EmitServiceEndpointBuilder(description);
                channelType = serviceBuilder.Build(ProxyAssembly.DefaultModule, logger);
            }
        }

        return new EmitServiceEndpointBinder<TService>(description, serviceInstanceType, contractType, channelType, logger);
    }

    public static IOperationDescriptor GenerateOperationDescriptor(Func<MethodInfo> getContractMethod) => new ReflectOperationDescriptor(getContractMethod);

    private static ContractDescription<Type> CreateDescription(Type serviceType, ILogger? logger)
    {
        var contractDescription = ContractDescriptionBuilder.Build(serviceType);

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

    private static (ContractDescription<Type> Description, Type ContractType) GenerateContract(Type serviceType, ILogger? logger)
    {
        var className = ContractDescriptionBuilder.GetContractClassName(serviceType);
        var contractType = ProxyAssembly.DefaultModule.GetType(className, false, false);

        ContractDescription<Type> description;
        if (contractType == null)
        {
            description = CreateDescription(serviceType, logger);
            contractType = new EmitContractBuilder(description).Build(ProxyAssembly.DefaultModule);
        }
        else
        {
            description = CreateDescription(serviceType, null);
        }

        return (description, contractType);
    }
}