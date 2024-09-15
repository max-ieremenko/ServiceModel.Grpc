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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp;

public static class AttributeAnalyzer
{
    public const string ImportAttributeName = "ImportGrpcService";
    public const string ExportAttributeName = "ExportGrpcService";
    public const string ExtensionAttributeName = "DesignTimeExtension";

    private const string ImportAttributeFullName = $"ServiceModel.Grpc.DesignTime.{ImportAttributeName}Attribute";
    private const string ExportAttributeFullName = $"ServiceModel.Grpc.DesignTime.{ExportAttributeName}Attribute";
    private const string ExtensionAttributeFullName = $"ServiceModel.Grpc.DesignTime.{ExtensionAttributeName}Attribute<";

    public static Type? TryImportGrpcService(AttributeData attribute) =>
        IsImportExportAttribute(attribute, ImportAttributeName, ImportAttributeFullName) ? typeof(ImportGrpcService) : null;

    public static Type? TryExportGrpcService(AttributeData attribute) =>
        IsImportExportAttribute(attribute, ExportAttributeName, ExportAttributeFullName) ? typeof(ExportGrpcService) : null;

    public static ITypeSymbol? TryExtension(AttributeData attribute)
    {
        if (attribute.AttributeClass == null
            || !attribute.AttributeClass.IsGenericType
            || attribute.AttributeClass.Name.IndexOf(ExtensionAttributeName, StringComparison.Ordinal) < 0
            || !attribute.AttributeClass.ToDisplayString(NullableFlowState.None).StartsWith(ExtensionAttributeFullName, StringComparison.Ordinal))
        {
            return null;
        }

        var genericArgs = attribute.AttributeClass.GenericTypeArguments();
        if (genericArgs.Length == 1 && genericArgs[0].IsAssignableFrom(typeof(IExtensionProvider)))
        {
            return genericArgs[0];
        }

        return null;
    }

    private static bool IsKnownAttribute(AttributeData attribute, string name, string fullName) =>
        attribute.AttributeClass != null
        && attribute.AttributeClass.Name.IndexOf(name, StringComparison.Ordinal) >= 0
        && !attribute.AttributeClass.IsGenericType
        && attribute.AttributeClass.ToDisplayString(NullableFlowState.None).Equals(fullName, StringComparison.Ordinal);

    private static bool IsImportExportAttribute(AttributeData attribute, string name, string fullName) =>
        IsKnownAttribute(attribute, name, fullName)
        && attribute.ConstructorArguments.Length == 1
        && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Type
        && attribute.ConstructorArguments[0].Value is INamedTypeSymbol;
}