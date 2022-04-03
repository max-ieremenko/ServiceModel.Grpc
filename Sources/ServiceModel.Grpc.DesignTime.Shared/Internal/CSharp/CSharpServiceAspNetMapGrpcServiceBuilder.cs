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

using System.Collections.Generic;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpServiceAspNetMapGrpcServiceBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;
        private readonly bool _isStaticClass;

        public CSharpServiceAspNetMapGrpcServiceBuilder(ContractDescription contract, bool isStaticClass)
        {
            _contract = contract;
            _isStaticClass = isStaticClass;
        }

        public override string GetGeneratedMemberName() => "Map" + _contract.BaseClassName;

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .Append("public static ")
                .AppendTypeName("Microsoft.AspNetCore.Builder", "GrpcServiceEndpointConventionBuilder")
                .Append(" ")
                .Append(GetGeneratedMemberName())
                .Append("(");

            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendTypeName("Microsoft.AspNetCore.Routing", "IEndpointRouteBuilder")
                .AppendLine(" builder)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output
                    .AppendArgumentNullException("builder")
                    .Append("return ")
                    .AppendTypeName("Microsoft.AspNetCore.Builder", "ServiceModelGrpcEndpointRouteBuilderExtensions")
                    .Append(".MapGrpcService<")
                    .Append(_contract.ContractInterfaceName)
                    .Append(", ")
                    .Append(_contract.EndpointBinderClassName)
                    .AppendLine(">(builder);");
            }

            Output.AppendLine("}");
        }
    }
}
