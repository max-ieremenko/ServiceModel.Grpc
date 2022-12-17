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

internal sealed class CSharpServiceSelfHostAddTransientServiceBuilder : CodeGeneratorBase
{
    private readonly ContractDescription _contract;
    private readonly bool _isStaticClass;

    public CSharpServiceSelfHostAddTransientServiceBuilder(ContractDescription contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public override string GetGeneratedMemberName() => "AddTransient" + _contract.BaseClassName;

    protected override void Generate()
    {
        WriteMetadata();
        Output
            .Append("public static ")
            .AppendTypeName("Grpc.Core", "Server.ServiceDefinitionCollection")
            .Append(" Add")
            .Append(_contract.BaseClassName)
            .Append("(");

        if (_isStaticClass)
        {
            Output.Append("this ");
        }

        Output
            .AppendTypeName("Grpc.Core", "Server.ServiceDefinitionCollection")
            .Append(" services, Func<")
            .Append(_contract.ContractInterfaceName)
            .AppendLine("> serviceFactory, Action<global::Grpc.Core.ServiceModelGrpcServiceOptions> configure = default)")
            .AppendLine("{");

        using (Output.Indent())
        {
            Output
                .Append("return ")
                .AppendTypeName("Grpc.Core", "ServiceDefinitionCollectionExtensions")
                .Append(".AddServiceModelTransient<")
                .Append(_contract.ContractInterfaceName)
                .Append(">(services, serviceFactory, new ")
                .Append(_contract.EndpointBinderClassName)
                .AppendLine("(), configure);");
        }

        Output.AppendLine("}");
    }
}