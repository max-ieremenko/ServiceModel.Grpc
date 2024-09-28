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

using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class MessagePackContractCodeGenerator : ICodeGenerator
{
    public const string PartialCctorMethodName = "RegisterMessagePackFormatters";

    private readonly IContractDescription _contract;
    private readonly List<IMessageDescription> _messages;

    public MessagePackContractCodeGenerator(IContractDescription contract, List<IMessageDescription> messages)
    {
        _contract = contract;
        _messages = messages;
    }

    public string GetHintName() => Hints.Contracts(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .AppendLine("// MessagePack extensions")
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
                    RegisterMessageFormatter(output, message);
                }
                else
                {
                    RegisterFormatter(output, message);
                }
            }
        }

        output.AppendLine("}");
    }

    private void RegisterFormatter(ICodeStringBuilder output, IMessageDescription message)
    {
        output
            .WriteTypeName("ServiceModel.Grpc.Configuration", "MessagePackMarshaller")
            .Append(".RegisterFormatter<")
            .WriteMessage(message)
            .Append(", ")
            .WriteMessage(message, "MessagePackFormatter")
            .AppendLine(">();");
    }

    private void RegisterMessageFormatter(ICodeStringBuilder output, IMessageDescription message)
    {
        output
            .WriteTypeName("ServiceModel.Grpc.Configuration", "MessagePackMarshaller")
            .Append(".RegisterMessageFormatter<");

        for (var i = 0; i < message.Properties.Length; i++)
        {
            output
                .WriteCommaIf(i > 0)
                .WriteType(message.Properties[i]);
        }

        output.AppendLine(">();");
    }
}