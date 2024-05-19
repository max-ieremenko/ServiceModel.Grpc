// <copyright>
// Copyright 2020-2024 Max Ieremenko
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
using System.Text;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

internal static class InternalDiagnosticReport
{
    private const string ExtensionTypeError = "GrpcDesignTime05";
    private const string ExtensionActivationError = "GrpcDesignTime06";

    public static void ReportExtensionTypeError(
        this IExtensionContext context,
        AttributeData attribute,
        string extensionType,
        Exception error)
    {
        context.ReportDiagnostic(
            DiagnosticReport.CreateDescriptor(
                ExtensionTypeError,
                DiagnosticSeverity.Error,
                new StringBuilder($"Fail to resolve type {extensionType}.").AddError(error).ToString()),
            attribute);
    }

    public static void ReportExtensionActivationError(
        this IExtensionContext context,
        AttributeData attribute,
        Type extensionType,
        Exception error)
    {
        context.ReportDiagnostic(
            DiagnosticReport.CreateDescriptor(
                ExtensionActivationError,
                DiagnosticSeverity.Error,
                new StringBuilder($"Fail to create instance of {extensionType.AssemblyQualifiedName}.").AddError(error).ToString()),
            attribute);
    }

    private static StringBuilder AddError(this StringBuilder text, Exception error)
    {
        text
            .Append(" Exception was of type '")
            .Append(error.GetType().Name)
            .Append("' with message '")
            .Append(error.Message)
            .Append("'.");

        if (error.InnerException != null)
        {
            text.Append(" => ");
            AddError(text, error.InnerException);
        }

        return text;
    }
}