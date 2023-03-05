// <copyright>
// Copyright 2020-2023 Max Ieremenko
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
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal sealed class CSharpClientBuilderBuilder : CodeGeneratorBase
{
    private readonly ContractDescription _contract;

    public CSharpClientBuilderBuilder(ContractDescription contract)
    {
        _contract = contract;
    }

    public override string GetGeneratedMemberName() => _contract.ClientBuilderClassName;

    protected override void Generate()
    {
        WriteMetadata();
        Output
            .Append("public sealed class ")
            .Append(_contract.ClientBuilderClassName)
            .Append(" : ")
            .AppendType(typeof(IClientBuilder<>))
            .Append(_contract.ContractInterfaceName)
            .AppendLine(">");
        Output.AppendLine("{");

        using (Output.Indent())
        {
            BuildFields();
            Output.AppendLine();

            BuildCtor();
            Output.AppendLine();

            BuildMethodInitialize();
            Output.AppendLine();

            BuildMethodBuild();
        }

        Output.AppendLine("}");
    }

    private void BuildCtor()
    {
        Output
            .Append("public ")
            .Append(_contract.ClientBuilderClassName)
            .AppendLine("()")
            .AppendLine("{")
            .AppendLine("}");
    }

    private void BuildFields()
    {
        Output
            .Append("private ")
            .Append(_contract.ContractClassName)
            .AppendLine(" _contract;");

        Output
            .Append("private Func<")
            .AppendType(typeof(CallOptions))
            .AppendLine("> _defaultCallOptionsFactory;");

        Output
            .Append("private ")
            .AppendType(typeof(IClientCallFilterHandlerFactory))
            .AppendLine(" _filterHandlerFactory;");
    }

    private void BuildMethodInitialize()
    {
        Output
            .Append("public void Initialize(")
            .AppendType(typeof(IClientMethodBinder))
            .AppendLine(" methodBinder)");

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output
                .AppendArgumentNullException("methodBinder");

            Output
                .Append("_contract = new ")
                .Append(_contract.ContractClassName)
                .Append("(methodBinder.")
                .Append(nameof(IClientMethodBinder.MarshallerFactory))
                .AppendLine(");");

            Output
                .Append("_defaultCallOptionsFactory = methodBinder.")
                .Append(nameof(IClientMethodBinder.DefaultCallOptionsFactory))
                .AppendLine(";");

            Output
                .AppendLine()
                .Append("if (methodBinder.")
                .Append(nameof(IClientMethodBinder.RequiresMetadata))
                .AppendLine(")")
                .AppendLine("{");
            using (Output.Indent())
            {
                foreach (var interfaceDescription in _contract.Services)
                {
                    foreach (var method in interfaceDescription.Operations)
                    {
                        Output
                            .Append("methodBinder.")
                            .Append(nameof(IClientMethodBinder.Add))
                            .Append("(_contract.")
                            .Append(method.GrpcMethodName)
                            .Append(", ")
                            .Append(_contract.ContractClassName)
                            .Append(".")
                            .Append(method.ClrDefinitionMethodName)
                            .AppendLine(");");
                    }

                    foreach (var entry in interfaceDescription.SyncOverAsync)
                    {
                        Output
                            .Append("methodBinder.")
                            .Append(nameof(IClientMethodBinder.Add))
                            .Append("(_contract.")
                            .Append(entry.Async.GrpcMethodName)
                            .Append(", ")
                            .Append(_contract.ContractClassName)
                            .Append(".")
                            .Append(entry.Sync.ClrDefinitionMethodName)
                            .AppendLine(");");
                    }
                }
            }

            Output
                .AppendLine("}")
                .AppendLine();

            Output
                .Append("_filterHandlerFactory = methodBinder.")
                .Append(nameof(IClientMethodBinder.CreateFilterHandlerFactory))
                .AppendLine("();");
        }

        Output.AppendLine("}");
    }

    private void BuildMethodBuild()
    {
        Output
            .Append("public ")
            .Append(_contract.ContractInterfaceName)
            .Append(" Build(")
            .AppendType(typeof(CallInvoker))
            .AppendLine(" callInvoker)");

        Output.AppendLine("{");
        using (Output.Indent())
        {
            Output
                .AppendArgumentNullException("callInvoker");

            Output
                .Append("return new ")
                .Append(_contract.ClientClassName)
                .AppendLine("(callInvoker, _contract, _defaultCallOptionsFactory, _filterHandlerFactory);");
        }

        Output.AppendLine("}");
    }
}