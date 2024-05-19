// <copyright>
// Copyright 2022-2024 Max Ieremenko
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

internal sealed class EndpointSelfHostBinderBaseBindTransientCodeGenerator : ICodeGenerator
{
    private readonly IContractDescription _contract;
    private readonly bool _isStaticClass;

    public EndpointSelfHostBinderBaseBindTransientCodeGenerator(IContractDescription contract, bool isStaticClass)
    {
        _contract = contract;
        _isStaticClass = isStaticClass;
    }

    public string GetHintName() => Hints.EndpointSelfHostBinderBaseBindTransient(_contract.BaseClassName);

    public void Generate(ICodeStringBuilder output)
    {
        output
            .WriteMetadata()
            .Append("public static ")
            .WriteTypeName("Grpc.Core", "ServiceBinderBase")
            .Append(" Bind")
            .Append(_contract.BaseClassName)
            .Append("(");

        if (_isStaticClass)
        {
            output.Append("this ");
        }

        output
            .WriteTypeName("Grpc.Core", "ServiceBinderBase")
            .Append(" serviceBinder, Func<")
            .WriteType(_contract.ContractInterface)
            .AppendLine("> serviceFactory, Action<global::Grpc.Core.ServiceModelGrpcServiceOptions> configure = default)")
            .AppendLine("{");

        using (output.Indent())
        {
            output
                .Append("return ")
                .WriteTypeName("Grpc.Core", "ServiceModelGrpcServiceBinderExtensions")
                .Append(".BindServiceModel<")
                .WriteType(_contract.ContractInterface)
                .Append(">(serviceBinder, new ")
                .Append(NamingConventions.EndpointBinder.Class(_contract.BaseClassName))
                .AppendLine("(), serviceFactory, configure);");
        }

        output.AppendLine("}");
    }
}