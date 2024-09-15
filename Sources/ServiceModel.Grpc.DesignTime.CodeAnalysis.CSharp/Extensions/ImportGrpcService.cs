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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

internal sealed class ImportGrpcService : IExtensionProvider
{
    private const string PropertyGenerateDiExtensions = "GenerateDependencyInjectionExtensions";

    public void ProvideExtensions(ExtensionProviderDeclaration declaration, IExtensionCollection extensions, IExtensionContext context)
    {
        var serviceType = (INamedTypeSymbol)declaration.Attribute.ConstructorArguments[0].Value!;
        if (!context.DescriptionExtensions.IsServiceContractInterface(serviceType))
        {
            context.ReportIsNotServiceContract(declaration.Attribute, serviceType);
            return;
        }

        extensions.TryAddContractDescription(serviceType, declaration.Attribute);
        extensions.TryAdd<MessageCodeGeneratorExtension>();
        extensions.TryAdd<ContractCodeGeneratorExtension>();
        extensions.Add(new ClientCodeGeneratorExtension(
            serviceType,
            GetAttributeValue(declaration, PropertyGenerateDiExtensions),
            declaration.DeclaredType.IsStatic));
    }

    private static bool GetAttributeValue(ExtensionProviderDeclaration declaration, string name)
    {
        if (declaration.Attribute.TryGetNamedArgumentValue(name, out var constant)
            && constant.TryGetPrimitiveValue<bool>(out var value))
        {
            return value;
        }

        return false;
    }
}