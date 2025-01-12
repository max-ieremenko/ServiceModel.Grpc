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

using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class MessagePackMessageFormatterCodeGenerator : ICodeGenerator
{
    private readonly int _propertiesCount;

    public MessagePackMessageFormatterCodeGenerator(int propertiesCount)
    {
        _propertiesCount = propertiesCount;
    }

    public string GetHintName() => Hints.Messages;

    public void Generate(ICodeStringBuilder output)
    {
        output
            .AppendLine("// MessagePack extensions")
            .Append("internal sealed class ")
            .WriteGenericMessage(_propertiesCount, "MessagePackFormatter")
            .Append(" : ")
            .WriteTypeName("MessagePack.Formatters", "IMessagePackFormatter")
            .Append("<")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine(">")
            .AppendLine("{");

        using (output.Indent())
        {
            BuildSerialize(output);
            output.AppendLine();
            BuildDeserialize(output);
        }

        output.AppendLine("}");
    }

    private void BuildSerialize(ICodeStringBuilder output)
    {
        output
            .Append("public void Serialize(ref ")
            .WriteTypeName("MessagePack", "MessagePackWriter")
            .Append(" writer, ")
            .WriteGenericMessage(_propertiesCount)
            .Append(" value, ")
            .WriteTypeName("MessagePack", "MessagePackSerializerOptions")
            .AppendLine(" options)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("if (value == null)")
                .AppendLine("{");
            using (output.Indent())
            {
                output
                    .AppendLine("writer.WriteNil();")
                    .AppendLine("return;");
            }

            output
                .AppendLine("}")
                .AppendLine();

            output
                .AppendFormat("writer.WriteArrayHeader({0});", _propertiesCount + 1)
                .AppendLine()
                .AppendLine("writer.WriteNil();");

            for (var i = 0; i < _propertiesCount; i++)
            {
                output
                    .WriteTypeName("MessagePack", "FormatterResolverExtensions")
                    .AppendFormat(".GetFormatterWithVerify<T{0}>(options.Resolver).Serialize(ref writer, value.Value{0}, options);", i + 1)
                    .AppendLine();
            }
        }

        output.AppendLine("}");
    }

    private void BuildDeserialize(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .WriteGenericMessage(_propertiesCount)
            .Append(" Deserialize(ref ")
            .WriteTypeName("MessagePack", "MessagePackReader")
            .Append(" reader, ")
            .WriteTypeName("MessagePack", "MessagePackSerializerOptions")
            .AppendLine(" options)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("if (reader.TryReadNil()) return null;")
                .AppendLine();

            output
                .AppendLine("options.Security.DepthStep(ref reader);")
                .AppendLine("var length = reader.ReadArrayHeader();")
                .Append("var result = new ")
                .WriteGenericMessage(_propertiesCount)
                .AppendLine("();")
                .AppendLine("for (var i = 0; i < length; i++)")
                .AppendLine("{");

            using (output.Indent())
            {
                output
                    .AppendLine("switch (i)")
                    .AppendLine("{");

                using (output.Indent())
                {
                    for (var i = 0; i < _propertiesCount; i++)
                    {
                        output
                            .AppendFormat("case {0}:", i + 1)
                            .AppendLine();

                        using (output.Indent())
                        {
                            output
                                .AppendFormat("result.Value{0} = ", i + 1)
                                .WriteTypeName("MessagePack", "FormatterResolverExtensions")
                                .AppendFormat(".GetFormatterWithVerify<T{0}>(options.Resolver).Deserialize(ref reader, options);", i + 1)
                                .AppendLine()
                                .AppendLine("break;");
                        }
                    }

                    output.AppendLine("default:");
                    using (output.Indent())
                    {
                        output
                            .AppendLine("reader.Skip();")
                            .AppendLine("break;");
                    }
                }

                output.AppendLine("}");
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendLine("reader.Depth--;")
                .AppendLine("return result;");
        }

        output.AppendLine("}");
    }
}