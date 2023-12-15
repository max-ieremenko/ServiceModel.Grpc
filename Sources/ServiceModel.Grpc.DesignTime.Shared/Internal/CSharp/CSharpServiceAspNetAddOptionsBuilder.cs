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

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

internal sealed class CSharpServiceAspNetAddOptionsBuilder : CodeGeneratorBase
{
    private readonly ContractDescription _contract;
    private readonly bool _isStaticClass;

    public CSharpServiceAspNetAddOptionsBuilder(ContractDescription contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public override string GetGeneratedMemberName() => "Add" + _contract.BaseClassName + "Options";

    protected override void Generate()
    {
        WriteMetadata();
        Output
            .Append("public static ")
            .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" ")
            .Append(GetGeneratedMemberName())
            .Append("(");

        if (_isStaticClass)
        {
            Output.Append("this ");
        }

        Output
            .AppendTypeName("Microsoft.Extensions.DependencyInjection", "IServiceCollection")
            .Append(" services, Action<")
            .AppendTypeName("Microsoft.Extensions.DependencyInjection", "ServiceModelGrpcServiceOptions")
            .Append("<")
            .Append(_contract.ContractInterfaceName)
            .AppendLine(">> configure)")
            .AppendLine("{");

        using (Output.Indent())
        {
            Output
                .AppendArgumentNullException("services")
                .Append("return ")
                .AppendTypeName("Microsoft.Extensions.DependencyInjection", "ServiceCollectionExtensions")
                .Append(".AddServiceModelGrpcServiceOptions<")
                .Append(_contract.ContractInterfaceName)
                .AppendLine(">(services, configure);");
        }

        Output.AppendLine("}");
    }
}