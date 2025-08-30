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

using System.Globalization;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class NerdbankMessagePackContractCodeGenerator : ICodeGenerator
{
    public const string PartialCctorMethodName = "RegisterNerdbankMessagePackShapes";

    private readonly IContractDescription _contract;
    private readonly List<IMessageDescription> _messages;

    public NerdbankMessagePackContractCodeGenerator(IContractDescription contract, List<IMessageDescription> messages)
    {
        _contract = contract;
        _messages = messages;
    }

    public string GetHintName() => Hints.Contracts(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .AppendLine("// Nerdbank.MessagePack extensions")
            .Append("partial class ")
            .AppendLine(NamingContract.Contract.Class(_contract.BaseClassName))
            .AppendLine("{");

        using (output.Indent())
        {
            BuildCCtor(output);
        }

        output.AppendLine("}");
    }

    private void BuildCCtor(ICodeStringBuilder output)
    {
        output
            .Append("static partial void ")
            .Append(PartialCctorMethodName)
            .AppendLine("()")
            .AppendLine("{");

        using (output.Indent())
        {
            for (var i = 0; i < _messages.Count; i++)
            {
                var message = _messages[i];
                if (message.IsBuiltIn)
                {
                    RegisterMessageShape(output, message);
                }
                else
                {
                    RegisterShape(output, message);
                }
            }
        }

        output.AppendLine("}");
    }

    private void RegisterShape(ICodeStringBuilder output, IMessageDescription message)
    {
        output
            .Append("if (!")
            .WriteTypeName("ServiceModel.Grpc.Configuration", "NerdbankMessagePackMarshaller")
            .Append(".IsRegisteredMessage<")
            .WriteMessage(message)
            .AppendLine(">())")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .WriteTypeName("ServiceModel.Grpc.Configuration", "NerdbankMessagePackMarshaller")
                .Append(".NewMessageShapeBuilder<")
                .WriteMessage(message)
                .Append(">(")
                .Append(message.Properties.Length.ToString(CultureInfo.InvariantCulture))
                .AppendLine(")");
            using (output.Indent())
            {
                for (var i = 0; i < message.Properties.Length; i++)
                {
                    output
                        .Append(".AddProperty(")
                        .WriteMessage(message)
                        .AppendFormat(".GetValue{0}", i + 1)
                        .Append(", ")
                        .WriteMessage(message)
                        .AppendFormat(".SetValue{0}", i + 1)
                        .AppendLine(")");
                }

                output.AppendLine(".Register();");
            }
        }

        output.AppendLine("}");
    }

    private void RegisterMessageShape(ICodeStringBuilder output, IMessageDescription message)
    {
        output
            .WriteTypeName("ServiceModel.Grpc.Configuration", "NerdbankMessagePackMarshaller")
            .Append(".RegisterMessageShape<");

        for (var i = 0; i < message.Properties.Length; i++)
        {
            output
                .WriteCommaIf(i > 0)
                .WriteType(message.Properties[i]);
        }

        output.AppendLine(">();");
    }
}