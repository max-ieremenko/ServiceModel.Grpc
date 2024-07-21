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

using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions.Reflection;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

public static class ContractDescriptionBuilder
{
    public static IContractDescription Build(INamedTypeSymbol serviceType)
    {
        var result = ContractDescriptionBuilder<ITypeSymbol>.Build(serviceType, null, ReflectTypeSymbol.Instance);

        Array.Sort(result.Interfaces, Compare);
        Array.Sort(result.Services, Compare);

        foreach (var description in result.Interfaces.Concat(result.Services))
        {
            Array.Sort(description.Methods, Compare);
            Array.Sort(description.NotSupportedOperations, Compare);
            Array.Sort(description.Operations, Compare);
        }

        return new ContractDescription(result);
    }

    public static IOperationDescription BuildOperation(IMethodSymbol method, string serviceName, string operationName)
    {
        if (!ContractDescriptionBuilder<ITypeSymbol>.TryBuildOperation(
                new CodeAnalysisMethodInfo(method),
                serviceName,
                operationName,
                ReflectTypeSymbol.Instance,
                out var result,
                out var error))
        {
            throw new NotSupportedException(error);
        }

        return new OperationDescription(result);
    }

    private static int Compare(InterfaceDescription<ITypeSymbol> x, InterfaceDescription<ITypeSymbol> y) =>
        StringComparer.Ordinal.Compare(x.InterfaceType.Name, y.InterfaceType.Name);

    private static int Compare(NotSupportedMethodDescription<ITypeSymbol> x, NotSupportedMethodDescription<ITypeSymbol> y) =>
        StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name);

    private static int Compare(OperationDescription<ITypeSymbol> x, OperationDescription<ITypeSymbol> y) =>
        StringComparer.Ordinal.Compare(x.Method.Name, y.Method.Name);
}