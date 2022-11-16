// <copyright>
// Copyright 2022 Max Ieremenko
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
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal static class CodeStringBuilderExtensions
{
    public static CodeStringBuilder AppendAttribute(this CodeStringBuilder code, Type attributeType, params string[] args)
    {
        var name = attributeType.Name;
        if (name.EndsWith("Attribute", StringComparison.Ordinal))
        {
            name = name.Substring(0, attributeType.Name.Length - 9);
        }

        code
            .Append("[")
            .AppendTypeName(attributeType.Namespace, name);

        if (args.Length > 0)
        {
            code.Append("(");
            for (var i = 0; i < args.Length; i++)
            {
                code
                    .AppendCommaIf(i != 0)
                    .Append(args[i]);
            }

            code.Append(")");
        }

        return code.AppendLine("]");
    }

    public static CodeStringBuilder AppendArgumentNullException(this CodeStringBuilder code, string paramName)
    {
        return code
            .Append("if (")
            .Append(paramName)
            .Append(" == null) throw new ArgumentNullException(\"")
            .Append(paramName)
            .AppendLine("\");");
    }

    public static CodeStringBuilder AppendTypeName(this CodeStringBuilder code, string? typeNamespace, string name)
    {
        if (typeNamespace != null)
        {
            code
                .Append("global::")
                .Append(typeNamespace)
                .Append(".");
        }

        var index = name.IndexOf('`');
        if (index > 0)
        {
            code.Append(name.Substring(0, index));
            code.Append("<");
        }
        else
        {
            code.Append(name);
        }

        return code;
    }

    public static CodeStringBuilder AppendType(this CodeStringBuilder code, Type type)
    {
        return AppendTypeName(code, type.Namespace, type.Name);
    }

    public static CodeStringBuilder AppendMessage(this CodeStringBuilder code, MessageDescription message)
    {
        if (message.IsBuiltIn)
        {
            code
                .Append("global::")
                .Append(typeof(Message).Namespace)
                .Append(".");
        }

        code.Append(message.ClassName);
        return code;
    }

    public static CodeStringBuilder AppendMessageOrDefault(this CodeStringBuilder code, MessageDescription? message)
    {
        return AppendMessage(code, message ?? MessageDescription.Empty);
    }
}