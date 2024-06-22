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

public static class DescriptionExtensions
{
    public static bool IsServiceContractInterface(INamedTypeSymbol type) =>
        ContractDescriptionBuilder<ITypeSymbol>.IsServiceContractInterface(type, ReflectTypeSymbol.Instance);

    public static IMethodSymbol GetSource(this OperationDescription<ITypeSymbol> operation) => ((CodeAnalysisMethodInfo)operation.Method).Source;

    public static IMethodSymbol GetSource(this NotSupportedMethodDescription<ITypeSymbol> method) => ((CodeAnalysisMethodInfo)method.Method).Source;
}