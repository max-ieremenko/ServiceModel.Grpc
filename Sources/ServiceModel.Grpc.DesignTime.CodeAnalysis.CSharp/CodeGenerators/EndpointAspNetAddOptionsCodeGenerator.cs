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

using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

internal sealed class EndpointAspNetAddOptionsCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;
    private readonly bool _isStaticClass;

    public EndpointAspNetAddOptionsCodeGenerator(IContractDescription contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public string GetHintName() => Hints.EndpointAspNetAddOptions(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" Add")
            .Append(_contract.BaseClassName)
            .Append("Options(");

        if (_isStaticClass)
        {
            output.Append("this ");
        }

        output
            .WriteTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" services, Action<")
            .WriteTypeName("Microsoft.Extensions.DependencyInjection", "ServiceModelGrpcServiceOptions")
            .Append("<")
            .WriteType(_contract.ContractInterface)
            .AppendLine(">> configure)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .WriteArgumentNullException("services")
                .Append("return ")
                .WriteTypeName("Microsoft.Extensions.DependencyInjection", "ServiceCollectionExtensions")
                .Append(".AddServiceModelGrpcServiceOptions<")
                .WriteType(_contract.ContractInterface)
                .AppendLine(">(services, configure);");
        }

        output.AppendLine("}");
    }
}