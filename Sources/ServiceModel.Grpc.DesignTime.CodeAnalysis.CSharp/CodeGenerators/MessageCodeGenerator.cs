﻿// <copyright>
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

using System.Globalization;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class MessageCodeGenerator : ICodeGenerator
{
    private readonly int _propertiesCount;
    private readonly MessageCodeGeneratorMetadata? _metadata;

    public MessageCodeGenerator(int propertiesCount, MessageCodeGeneratorMetadata? metadata)
    {
        _propertiesCount = propertiesCount;
        _metadata = metadata;
    }

    public string GetHintName() => Hints.Messages;

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .WriteSerializableAttribute()
            .WriteDataContractAttribute()
            .Append("public sealed partial class ")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine()
            .AppendLine("{");

        using (output.Indent())
        {
            BuildCctor(output);
            BuildCtorDefault(output);
            output.AppendLine();

            BuildCtorFull(output);
            BuildProperties(output);
        }

        output.AppendLine("}");
    }

    private void BuildCctor(ICodeStringBuilder output)
    {
        var methods = _metadata?.GetPartialCctors();
        if (methods == null || methods.Count == 0)
        {
            return;
        }

        output
            .AppendLine("static Message()")
            .AppendLine("{");

        using (output.Indent())
        {
            foreach (var method in methods)
            {
                output
                    .Append(method)
                    .AppendLine("();");
            }
        }

        output
            .AppendLine("}")
            .AppendLine();

        foreach (var method in methods)
        {
            output
                .Append("static partial void ")
                .Append(method)
                .AppendLine("();");
        }

        output.AppendLine();
    }

    private void BuildCtorDefault(ICodeStringBuilder output)
    {
        output.AppendLine("public Message() { }");
    }

    private void BuildCtorFull(ICodeStringBuilder output)
    {
        output.Append("public Message(");

        for (var i = 0; i < _propertiesCount; i++)
        {
            output
                .WriteCommaIf(i != 0)
                .AppendFormat("T{0} value{0}", i + 1);
        }

        output
            .AppendLine(")")
            .AppendLine("{");

        using (output.Indent())
        {
            for (var i = 0; i < _propertiesCount; i++)
            {
                output
                    .AppendFormat("Value{0} = value{0}", i + 1)
                    .AppendLine(";");
            }
        }

        output.AppendLine("}");
    }

    private void BuildProperties(ICodeStringBuilder output)
    {
        for (var i = 0; i < _propertiesCount; i++)
        {
            var order = (i + 1).ToString(CultureInfo.InvariantCulture);
            output
                .AppendLine()
                .WriteDataMemberAttribute(order)
                .AppendFormat("public T{0} Value{0}", order)
                .AppendLine(" { get; set; }");
        }
    }
}