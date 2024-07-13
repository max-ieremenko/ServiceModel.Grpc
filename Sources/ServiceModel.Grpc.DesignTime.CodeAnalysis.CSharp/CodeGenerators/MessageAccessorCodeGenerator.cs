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
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class MessageAccessorCodeGenerator : ICodeGenerator
{
    private readonly int _propertiesCount;

    public MessageAccessorCodeGenerator(int propertiesCount)
    {
        _propertiesCount = propertiesCount;
    }

    public string GetHintName() => Hints.Messages;

    public void Generate(ICodeStringBuilder output)
    {
        output
            .Append("public sealed class ")
            .WriteGenericMessage(_propertiesCount, "Accessor")
            .Append(" : ")
            .WriteType(typeof(IMessageAccessor))
            .AppendLine()
            .AppendLine("{");

        using (output.Indent())
        {
            BuildCtor(output);
            output.AppendLine();

            BuildProperties(output);
            output.AppendLine();

            BuildCreateNew(output);
            output.AppendLine();

            BuildGetInstanceType(output);
            output.AppendLine();

            BuildGetValueType(output);
            output.AppendLine();

            BuildGetValue(output);
            output.AppendLine();

            BuildSetValue(output);
        }

        output.AppendLine("}");
    }

    private void BuildCtor(ICodeStringBuilder output)
    {
        output
            .AppendLine("public MessageAccessor(string[] names)")
            .AppendLine("{");

        using (output.Indent())
        {
            output.WriteArgumentNullException("names");

            output
                .AppendFormat("if (names.Length != {0})", _propertiesCount)
                .Append(" throw new ArgumentOutOfRangeException(\"names\");")
                .AppendLine();

            output.AppendLine("Names = names;");
        }

        output.AppendLine("}");
    }

    private void BuildProperties(ICodeStringBuilder output)
    {
        output.AppendLine("public string[] Names { get; }");
    }

    private void BuildCreateNew(ICodeStringBuilder output)
    {
        output
            .Append("public object CreateNew() => new ")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine("();");
    }

    private void BuildGetInstanceType(ICodeStringBuilder output)
    {
        output
            .Append("public Type GetInstanceType() => typeof(")
            .WriteGenericMessage(_propertiesCount)
            .AppendLine(");");
    }

    private void BuildGetValueType(ICodeStringBuilder output)
    {
        output
            .AppendLine("public Type GetValueType(int property)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .AppendLine("switch (property)")
                .AppendLine("{");

            using (output.Indent())
            {
                for (var i = 0; i < _propertiesCount; i++)
                {
                    output
                        .AppendFormat("case {0}:", i)
                        .AppendLine();

                    using (output.Indent())
                    {
                        output
                            .AppendFormat("return typeof(T{0});", i + 1)
                            .AppendLine();
                    }
                }
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendLine("throw new ArgumentOutOfRangeException(\"property\");");
        }

        output.AppendLine("}");
    }

    private void BuildGetValue(ICodeStringBuilder output)
    {
        output
            .AppendLine("public object GetValue(object message, int property)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .WriteArgumentNullException("message")
                .Append("var instance = (")
                .WriteGenericMessage(_propertiesCount)
                .AppendLine(")message;")
                .AppendLine("switch (property)")
                .AppendLine("{");

            using (output.Indent())
            {
                for (var i = 0; i < _propertiesCount; i++)
                {
                    output
                        .AppendFormat("case {0}:", i)
                        .AppendLine();

                    using (output.Indent())
                    {
                        output
                            .AppendFormat("return instance.Value{0};", i + 1)
                            .AppendLine();
                    }
                }
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendLine("throw new ArgumentOutOfRangeException(\"property\");");
        }

        output.AppendLine("}");
    }

    private void BuildSetValue(ICodeStringBuilder output)
    {
        output
            .AppendLine("public void SetValue(object message, int property, object value)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .WriteArgumentNullException("message")
                .Append("var instance = (")
                .WriteGenericMessage(_propertiesCount)
                .AppendLine(")message;")
                .AppendLine("switch (property)")
                .AppendLine("{");

            using (output.Indent())
            {
                for (var i = 0; i < _propertiesCount; i++)
                {
                    output
                        .AppendFormat("case {0}:", i)
                        .AppendLine();

                    using (output.Indent())
                    {
                        output
                            .AppendFormat("instance.Value{0} = (T{0})value;", i + 1)
                            .AppendLine()
                            .AppendLine("return;");
                    }
                }
            }

            output
                .AppendLine("}")
                .AppendLine()
                .AppendLine("throw new ArgumentOutOfRangeException(\"property\");");
        }

        output.AppendLine("}");
    }
}