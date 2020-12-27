// <copyright>
// Copyright 2020 Max Ieremenko
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

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpServiceSelfHostAddSingletonServiceBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;
        private readonly bool _isStaticClass;

        public CSharpServiceSelfHostAddSingletonServiceBuilder(ContractDescription contract, bool isStaticClass)
        {
            _contract = contract;
            _isStaticClass = isStaticClass;
        }

        public override string GetGeneratedMemberName() => "AddSingleton" + _contract.BaseClassName;

        protected override void Generate()
        {
            Output
                .Append("public static Server.ServiceDefinitionCollection Add")
                .Append(_contract.BaseClassName)
                .Append("(");

            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .Append("Server.ServiceDefinitionCollection services, ")
                .Append(_contract.ContractInterfaceName)
                .AppendLine(" service, Action<global::Grpc.Core.ServiceModelGrpcServiceOptions> configure = default)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output
                    .AppendLine("return services.AddServiceModelSingleton(service, configure);");
            }

            Output.AppendLine("}");
        }
    }
}
