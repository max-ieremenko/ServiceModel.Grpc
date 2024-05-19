// <copyright>
// Copyright 2022-2024 Max Ieremenko
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
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal static partial class CodeStringBuilderExtensions
{
    public static ICodeStringBuilder WriteMetadata(this ICodeStringBuilder output)
    {
        output
            .WriteAttribute(typeof(GeneratedCodeAttribute), "\"ServiceModel.Grpc\"", "\"" + typeof(CodeStringBuilderExtensions).Assembly.GetName().Version.ToString(3) + "\"")
            .WriteAttribute(typeof(CompilerGeneratedAttribute))
            .WriteAttribute(typeof(ExcludeFromCodeCoverageAttribute))
            .WriteAttribute(typeof(ObfuscationAttribute), "Exclude = true");

        return output;
    }

    public static ICodeStringBuilder WriteSerializableAttribute(this ICodeStringBuilder code) =>
        WriteAttribute(code, "System", "Serializable");

    public static ICodeStringBuilder WriteDataContractAttribute(this ICodeStringBuilder code) =>
        WriteAttribute(code, "System.Runtime.Serialization", "DataContract", "Name = \"m\"", "Namespace = \"s\"");

    public static ICodeStringBuilder WriteDataMemberAttribute(this ICodeStringBuilder code, string order) =>
        WriteAttribute(code, "System.Runtime.Serialization", "DataMember", $"Name = \"v{order}\"", $"Order = {order}");

    public static ICodeStringBuilder WriteAttribute(this ICodeStringBuilder code, Type attributeType, params string[] args) =>
        WriteAttribute(code, attributeType.Namespace, attributeType.Name, args);

    public static ICodeStringBuilder WriteAttribute(this ICodeStringBuilder output, string? typeNamespace, string name, params string[] args)
    {
        if (name.EndsWith("Attribute", StringComparison.Ordinal))
        {
            name = name.Substring(0, name.Length - 9);
        }

        output
            .Append("[")
            .WriteTypeName(typeNamespace, name);

        if (args.Length > 0)
        {
            output.Append("(");
            for (var i = 0; i < args.Length; i++)
            {
                output
                    .WriteCommaIf(i != 0)
                    .Append(args[i]);
            }

            output.Append(")");
        }

        return output.AppendLine("]");
    }
}