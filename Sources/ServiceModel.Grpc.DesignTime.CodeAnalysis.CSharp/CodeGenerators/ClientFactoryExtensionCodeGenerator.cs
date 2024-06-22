// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class ClientFactoryExtensionCodeGenerator : ICodeGenerator
{
    private readonly ContractDescription<ITypeSymbol> _contract;
    private readonly bool _isStaticClass;

    public ClientFactoryExtensionCodeGenerator(ContractDescription<ITypeSymbol> contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public string GetHintName() => Hints.ClientFactoryExtensions(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteType(typeof(IClientFactory))
            .Append(" Add")
            .Append(NamingConventions.Client.Class(_contract.BaseClassName))
            .Append("(");

        if (_isStaticClass)
        {
            output.Append("this ");
        }

        output
            .WriteType(typeof(IClientFactory))
            .Append(" clientFactory, Action<")
            .WriteType(typeof(ServiceModelGrpcClientOptions))
            .AppendLine("> configure = null)")
            .AppendLine("{");

        using (output.Indent())
        {
            output.WriteArgumentNullException("clientFactory");

            output
                .Append("clientFactory.")
                .Append(nameof(IClientFactory.AddClient))
                .Append("(new ")
                .Append(NamingConventions.ClientBuilder.Class(_contract.BaseClassName))
                .AppendLine("(), configure);");

            output.AppendLine("return clientFactory;");
        }

        output.AppendLine("}");
    }
}