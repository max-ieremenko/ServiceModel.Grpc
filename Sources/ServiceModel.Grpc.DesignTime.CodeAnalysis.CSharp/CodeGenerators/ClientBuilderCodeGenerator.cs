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

using Grpc.Core;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class ClientBuilderCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;

    public ClientBuilderCodeGenerator(IContractDescription contract)
    {
        _contract = contract;
    }

    public string GetHintName() => Hints.ClientBuilders(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public sealed class ")
            .Append(NamingContract.ClientBuilder.Class(_contract.BaseClassName))
            .Append(" : ")
            .WriteType(typeof(IClientBuilder<>))
            .WriteType(_contract.ContractInterface)
            .AppendLine(">");
        output.AppendLine("{");

        using (output.Indent())
        {
            BuildFields(output);
            output.AppendLine();

            BuildCtor(output);
            output.AppendLine();

            BuildMethodInitialize(output);
            output.AppendLine();

            BuildMethodBuild(output);
        }

        output.AppendLine("}");
    }

    private void BuildCtor(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .Append(NamingContract.ClientBuilder.Class(_contract.BaseClassName))
            .AppendLine("()")
            .AppendLine("{")
            .AppendLine("}");
    }

    private void BuildFields(ICodeStringBuilder output)
    {
        output
            .Append("private ")
            .Append(NamingContract.Contract.Class(_contract.BaseClassName))
            .AppendLine(" _contract;");

        output
            .Append("private ")
            .WriteType(typeof(IClientCallInvoker))
            .AppendLine(" _clientCallInvoker;");
    }

    private void BuildMethodInitialize(ICodeStringBuilder output)
    {
        output
            .Append("public void Initialize(")
            .WriteType(typeof(IClientMethodBinder))
            .AppendLine(" methodBinder)");

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("methodBinder");

            output
                .Append("_contract = new ")
                .Append(NamingContract.Contract.Class(_contract.BaseClassName))
                .Append("(methodBinder.")
                .Append(nameof(IClientMethodBinder.MarshallerFactory))
                .AppendLine(");");

            output
                .AppendLine()
                .Append("if (methodBinder.")
                .Append(nameof(IClientMethodBinder.RequiresMetadata))
                .AppendLine(")")
                .AppendLine("{");
            using (output.Indent())
            {
                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Operations)
                    {
                        output
                            .Append("methodBinder.")
                            .Append(nameof(IClientMethodBinder.Add))
                            .Append("(_contract.")
                            .Append(NamingContract.Contract.GrpcMethod(method.OperationName))
                            .Append(", ")
                            .Append(NamingContract.Contract.Class(_contract.BaseClassName))
                            .Append(".")
                            .Append(NamingContract.Contract.DescriptorMethod(method.OperationName))
                            .AppendLine("());");
                    }

                    foreach (var entry in interfaceDescription.SyncOverAsync)
                    {
                        output
                            .Append("methodBinder.")
                            .Append(nameof(IClientMethodBinder.Add))
                            .Append("(_contract.")
                            .Append(NamingContract.Contract.GrpcMethod(entry.Async.OperationName))
                            .Append(", ")
                            .Append(NamingContract.Contract.Class(_contract.BaseClassName))
                            .Append(".")
                            .Append(NamingContract.Contract.DescriptorMethodSync(entry.Async.OperationName))
                            .AppendLine("());");
                    }
                }
            }

            output
                .AppendLine("}")
                .AppendLine();

            output
                .Append("_clientCallInvoker = methodBinder.")
                .Append(nameof(IClientMethodBinder.CreateCallInvoker))
                .AppendLine("();");
        }

        output.AppendLine("}");
    }

    private void BuildMethodBuild(ICodeStringBuilder output)
    {
        output
            .Append("public ")
            .WriteType(_contract.ContractInterface)
            .Append(" Build(")
            .WriteType(typeof(CallInvoker))
            .AppendLine(" callInvoker)");

        output.AppendLine("{");
        using (output.Indent())
        {
            output.WriteArgumentNullException("callInvoker");

            output
                .Append("return new ")
                .Append(NamingContract.Client.Class(_contract.BaseClassName))
                .AppendLine("(callInvoker, _contract, _clientCallInvoker);");
        }

        output.AppendLine("}");
    }
}