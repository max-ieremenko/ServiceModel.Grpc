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

using System.Collections.Generic;

namespace ServiceModel.Grpc.DesignTime.Internal.CSharp
{
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

        public override IEnumerable<string> GetUsing()
        {
            yield return "Microsoft.Extensions.DependencyInjection";
        }

        protected override void Generate()
        {
            Output
                .Append("public static IServiceCollection ")
                .Append(GetGeneratedMemberName())
                .Append("(");

            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendLine("IServiceCollection services, Action<ServiceModelGrpcServiceOptions<")
                .Append(_contract.ContractInterfaceName)
                .AppendLine(">> configure)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output
                    .Append("return services.AddServiceModelGrpcServiceOptions<")
                    .Append(_contract.ContractInterfaceName)
                    .AppendLine(">(configure);");
            }

            Output.AppendLine("}");
        }
    }
}
