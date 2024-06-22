// <copyright>
// Copyright 2020-2024 Max Ieremenko
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

using Microsoft.CodeAnalysis;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class EndpointSelfHostAddSingletonCodeGenerator : ICodeGenerator
{
    private readonly ContractDescription<ITypeSymbol> _contract;
    private readonly bool _isStaticClass;

    public EndpointSelfHostAddSingletonCodeGenerator(ContractDescription<ITypeSymbol> contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public string GetHintName() => Hints.EndpointSelfHostAddSingleton(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("Grpc.Core", "Server.ServiceDefinitionCollection")
            .Append(" Add")
            .Append(_contract.BaseClassName)
            .Append("(");

        if (_isStaticClass)
        {
            output.Append("this ");
        }

        output
            .WriteTypeName("Grpc.Core", "Server.ServiceDefinitionCollection")
            .Append(" services, ")
            .WriteType(_contract.ContractInterface)
            .AppendLine(" service, Action<global::Grpc.Core.ServiceModelGrpcServiceOptions> configure = default)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .Append("return ")
                .WriteTypeName("Grpc.Core", "ServiceDefinitionCollectionExtensions")
                .Append(".AddServiceModelSingleton<")
                .WriteType(_contract.ContractInterface)
                .Append(">(services, service, new ")
                .Append(NamingConventions.EndpointBinder.Class(_contract.BaseClassName))
                .AppendLine("(), configure);");
        }

        output.AppendLine("}");
    }
}