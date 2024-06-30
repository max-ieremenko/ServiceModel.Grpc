// <copyright>
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

using System;
using System.Reflection;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions.Reflection;

namespace ServiceModel.Grpc.Emit.Descriptions;

internal static class ContractDescriptionBuilder
{
    public static ContractDescription<Type> Build(Type serviceType) =>
        ContractDescriptionBuilder<Type>.Build(serviceType, GetNamespace(serviceType), new ReflectType());

    public static OperationDescription<Type> BuildOperation(MethodInfo method, string serviceName, string operationName)
    {
        var result = ContractDescriptionBuilder<Type>.TryBuildOperation(
            new ReflectionMethodInfo(method),
            serviceName,
            operationName,
            new ReflectType(),
            out var operation,
            out var error);

        if (!result)
        {
            throw new NotSupportedException(error);
        }

        return operation!;
    }

    public static string GetClassName(Type serviceType) => NamingContract.GetBaseClassName(new ReflectType(), serviceType, GetNamespace(serviceType));

    public static string GetContractClassName(Type serviceType) => NamingContract.Contract.Class(GetClassName(serviceType));

    public static string GetClientBuilderClassName(Type serviceType) => NamingContract.ClientBuilder.Class(GetClassName(serviceType));

    public static string GetEndpointClassName(Type serviceType) => NamingContract.Endpoint.Class(GetClassName(serviceType));

    private static string GetNamespace(Type serviceType)
    {
        var assemblyName = serviceType.Assembly.GetName().Name;
        var ns = ReflectionTools.GetNamespace(serviceType);
        return $"{assemblyName}.{ns}";
    }
}