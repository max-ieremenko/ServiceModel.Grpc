// <copyright>
// Copyright 2024 Max Ieremenko
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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

public sealed class ExportGrpcService : IExtensionProvider
{
    private const string PropertyGenerateAspNetExtensions = "GenerateAspNetExtensions";
    private const string PropertyGenerateSelfHostExtensions = "GenerateSelfHostExtensions";

    public void ProvideExtensions(ExtensionProviderDeclaration declaration, IExtensionCollection extensions, IExtensionContext context)
    {
        var serviceType = (INamedTypeSymbol)declaration.Attribute.ConstructorArguments[0].Value!;

        extensions.TryAddContractDescription(serviceType, declaration.Attribute);
        extensions.TryAdd<MessageCodeGeneratorExtension>();
        extensions.TryAdd<ContractCodeGeneratorExtension>();
        extensions.Add(new EndpointCodeGeneratorExtension(
            serviceType,
            declaration.Attribute.GetNamedArgumentValue(PropertyGenerateAspNetExtensions, false),
            declaration.Attribute.GetNamedArgumentValue(PropertyGenerateSelfHostExtensions, false),
            declaration.DeclaredType.IsStatic));
    }
}