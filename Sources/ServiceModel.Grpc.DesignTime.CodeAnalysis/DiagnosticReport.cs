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

public static class DiagnosticReport
{
    private const string DefaultTitle = "GrpcDesignTime";
    private const string DefaultCategory = "GrpcDesignTime";

    private const string IsNotServiceContract = "GrpcDesignTime01";
    private const string InheritsNotServiceContract = "GrpcDesignTime02";
    private const string IsNotOperationContract = "GrpcDesignTime03";
    private const string IsNotSupportedOperation = "GrpcDesignTime04";

    public static void ReportIsNotServiceContract(
        this IExtensionContext context,
        AttributeData attribute,
        INamedTypeSymbol serviceType)
    {
        context.ReportDiagnostic(
            CreateDescriptor(
                IsNotServiceContract,
                DiagnosticSeverity.Error,
                $"{serviceType.Name} is not service contract."),
            attribute);
    }

    public static void ReportInheritsNotServiceContract(
        this IExtensionContext context,
        AttributeData attribute,
        ITypeSymbol serviceType,
        ITypeSymbol parent)
    {
        context.ReportDiagnostic(
            CreateDescriptor(
                InheritsNotServiceContract,
                DiagnosticSeverity.Info,
                $"{serviceType.Name}: {parent.Name} is not service contract."),
            attribute);
    }

    public static void ReportIsNotOperationContract(
        this IExtensionContext context,
        AttributeData attribute,
        INamedTypeSymbol serviceType,
        string description)
    {
        context.ReportDiagnostic(
            CreateDescriptor(
                IsNotOperationContract,
                DiagnosticSeverity.Info,
                $"{serviceType.Name}: {description}"),
            attribute);
    }

    public static void ReportIsNotSupportedOperation(
        this IExtensionContext context,
        AttributeData attribute,
        INamedTypeSymbol serviceType,
        string description)
    {
        context.ReportDiagnostic(
            CreateDescriptor(
                IsNotSupportedOperation,
                DiagnosticSeverity.Warning,
                $"{serviceType.Name}: {description}"),
            attribute);
    }

    public static DiagnosticDescriptor CreateDescriptor(string id, DiagnosticSeverity severity, string message) => new(
        id,
        DefaultTitle,
        message,
        DefaultCategory,
        severity,
        true);

    public static void ReportDiagnostic(this IExtensionContext context, DiagnosticDescriptor descriptor, AttributeData attribute)
    {
        var attributeNode = attribute.ApplicationSyntaxReference?.GetSyntax();

        Location? location = null;
        if (attributeNode != null)
        {
            var attributeLocation = attributeNode.GetLocation();
            location = Location.Create(attributeNode.SyntaxTree.FilePath, attributeLocation.SourceSpan, attributeLocation.GetLineSpan().Span);
        }

        var diagnostic = Diagnostic.Create(descriptor, location);
        context.ReportDiagnostic(diagnostic);
    }
}