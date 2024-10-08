﻿// <copyright>
// Copyright Max Ieremenko
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

using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.CodeGenerators;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit;

internal static class EmitGenerator
{
    [UnconditionalSuppressMessage("Trimming", "IL2072:Activator.CreateInstance")]
    [UnconditionalSuppressMessage("Trimming", "IL2067:Activator.CreateInstance")]
    public static IClientBuilder<TContract> GenerateClientBuilder<TContract>(ILogger? logger)
    {
        var serviceType = typeof(TContract);

        Type? clientBuilderType;
        lock (ProxyAssembly.SyncRoot)
        {
            if (!ProxyAssembly.DefaultModule.TryGetType(ContractDescriptionBuilder.GetClientBuilderClassName(serviceType), out clientBuilderType))
            {
                var (description, contractType) = GenerateContract(serviceType, logger);
                var clientType = new EmitClientBuilder(description, contractType).Build(ProxyAssembly.DefaultModule);
                clientBuilderType = new EmitClientBuilderBuilder(description, contractType, clientType).Build(ProxyAssembly.DefaultModule);
            }
        }

        return (IClientBuilder<TContract>)Activator.CreateInstance(clientBuilderType)!;
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
            if (!ProxyAssembly.DefaultModule.TryGetType(ContractDescriptionBuilder.GetEndpointClassName(serviceType), out channelType))
            {
                var serviceBuilder = new EmitServiceEndpointBuilder(description);
                channelType = serviceBuilder.Build(ProxyAssembly.DefaultModule, logger);
            }
        }

        return new EmitServiceEndpointBinder<TService>(description, serviceInstanceType, contractType, channelType, logger);
    }

    // only for tests
    public static Type GenerateContract<TService>()
    {
        lock (ProxyAssembly.SyncRoot)
        {
            return GenerateContract(typeof(TService), null).ContractType;
        }
    }

    private static ContractDescription<Type> CreateDescription(Type serviceType, ILogger? logger)
    {
        var contractDescription = ContractDescriptionBuilder.Build(serviceType);
        if (logger == null)
        {
            return contractDescription;
        }

        foreach (var interfaceDescription in contractDescription.Interfaces)
        {
            logger.LogDebug("{0}: {1} is not service contract.", serviceType.FullName, interfaceDescription.InterfaceType.FullName);
        }

        foreach (var interfaceDescription in contractDescription.Services)
        {
            foreach (var method in interfaceDescription.Methods)
            {
                logger.LogDebug("{0}: {1}", serviceType.FullName, method.Error);
            }

            foreach (var method in interfaceDescription.NotSupportedOperations)
            {
                logger.LogError("{0}: {1}", serviceType.FullName, method.Error);
            }
        }

        return contractDescription;
    }

    private static (ContractDescription<Type> Description, Type ContractType) GenerateContract(Type serviceType, ILogger? logger)
    {
        var className = ContractDescriptionBuilder.GetContractClassName(serviceType);

        ContractDescription<Type> description;
        if (ProxyAssembly.DefaultModule.TryGetType(className, out var contractType))
        {
            description = CreateDescription(serviceType, null);
        }
        else
        {
            description = CreateDescription(serviceType, logger);
            contractType = EmitContractBuilder.Build(ProxyAssembly.DefaultModule, description);
        }

        return (description, contractType);
    }
}