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

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

internal static class AttributeAnalyzer
{
    private const string ImportAttributeFullName = "ServiceModel.Grpc.DesignTime.ImportGrpcServiceAttribute";
    private const string ExportAttributeFullName = "ServiceModel.Grpc.DesignTime.ExportGrpcServiceAttribute";
    private const string ExtensionAttributeFullName = "ServiceModel.Grpc.DesignTime.DesignTimeExtensionAttribute<";

    public static bool TryGetProviderType(TypeHandler typeHandler, AttributeData attribute, out ITypeSymbol? typeSymbol, out Type? type)
    {
        typeSymbol = null;
        type = null;

        var fullName = attribute.AttributeClass?.ToDisplayString(NullableFlowState.None);
        if (string.IsNullOrEmpty(fullName))
        {
            return false;
        }

        if (ImportAttributeFullName.Equals(fullName, StringComparison.Ordinal))
        {
            if (!IsImportExportAttribute(attribute))
            {
                return false;
            }

            type = typeHandler.ImportGrpcService;
            return true;
        }

        if (ExportAttributeFullName.Equals(fullName, StringComparison.Ordinal))
        {
            if (!IsImportExportAttribute(attribute))
            {
                return false;
            }

            type = typeHandler.ExportGrpcService;
            return true;
        }

        if (!fullName!.StartsWith(ExtensionAttributeFullName, StringComparison.Ordinal))
        {
            return false;
        }

        var genericArgs = attribute.AttributeClass!.GenericTypeArguments();
        if (genericArgs.Length == 1 && genericArgs[0].IsAssignableFrom(typeof(IExtensionProvider)))
        {
            typeSymbol = genericArgs[0];
            return true;
        }

        return false;
    }

    private static bool IsImportExportAttribute(AttributeData attribute) =>
        attribute.AttributeClass != null
        && !attribute.AttributeClass.IsGenericType
        && attribute.ConstructorArguments.Length == 1
        && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Type
        && attribute.ConstructorArguments[0].Value is INamedTypeSymbol;
}