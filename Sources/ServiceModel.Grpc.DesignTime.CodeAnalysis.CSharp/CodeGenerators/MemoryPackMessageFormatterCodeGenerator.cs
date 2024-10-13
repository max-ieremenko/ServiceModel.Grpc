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

internal sealed class MemoryPackMessageFormatterCodeGenerator : ICodeGenerator
{
    private readonly int _propertiesCount;

    public MemoryPackMessageFormatterCodeGenerator(int propertiesCount)
    {
        _propertiesCount = propertiesCount;
    }

    public string GetHintName() => Hints.Messages;

    public void Generate(ICodeStringBuilder output)
    {
        output
            .AppendLine("// MemoryPack extensions")
            .AppendLine("[MemoryPack.Internal.Preserve]")
            .Append("private sealed class ")
            .WriteGenericMessage(_propertiesCount, "MemoryPackFormatter")
            .Append(" : ")
            .WriteTypeName("MemoryPack", "MemoryPackFormatter")
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
            .AppendLine("[MemoryPack.Internal.Preserve]")
            .Append("public override void Serialize<TBufferWriter>(ref ")
            .WriteTypeName("MemoryPack", "MemoryPackWriter")
            .Append("<TBufferWriter> writer, scoped ref ")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine(" value)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("if (value == null)")
                .AppendLine("{");
            using (output.Indent())
            {
                output
                    .AppendLine("writer.WriteNullObjectHeader();")
                    .AppendLine("return;");
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendFormat("writer.WriteObjectHeader({0});", _propertiesCount)
                .AppendLine();

            for (var i = 0; i < _propertiesCount; i++)
            {
                output
                    .AppendFormat("writer.WriteValue(value.Value{0});", i + 1)
                    .AppendLine();
            }
        }

        output.AppendLine("}");
    }

    private void BuildDeserialize(ICodeStringBuilder output)
    {
        output
            .AppendLine("[MemoryPack.Internal.Preserve]")
            .Append("public override void Deserialize(ref ")
            .WriteTypeName("MemoryPack", "MemoryPackReader")
            .Append(" reader, scoped ref ")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine(" value)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("if (!reader.TryReadObjectHeader(out var length))")
                .AppendLine("{");
            using (output.Indent())
            {
                output
                    .AppendLine("value = default;")
                    .AppendLine("return;");
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendFormat("if (length > {0}) MemoryPack.MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(", _propertiesCount)
                .WriteGenericMessage(_propertiesCount)
                .AppendFormat("), {0}, length);", _propertiesCount)
                .AppendLine()
                .AppendLine();

            output
                .Append("var result = value ?? new ")
                .WriteGenericMessage(_propertiesCount)
                .AppendLine("();");

            for (var i = 0; i < _propertiesCount; i++)
            {
                output
                    .AppendFormat("if (length > {0})", i)
                    .AppendLine()
                    .AppendLine("{");
                using (output.Indent())
                {
                    output
                        .AppendFormat("result.Value{0} = reader.ReadValue<T{0}>();", i + 1)
                        .AppendLine();
                }

                output
                    .AppendLine("}")
                    .AppendLine("else if (value != null)")
                    .AppendLine("{");
                using (output.Indent())
                {
                    output
                        .AppendFormat("result.Value{0} = default;", i + 1)
                        .AppendLine();
                }

                output.AppendLine("}");
            }

            output
                .AppendLine()
                .AppendLine("value = result;");
        }

        output.AppendLine("}");
    }
}