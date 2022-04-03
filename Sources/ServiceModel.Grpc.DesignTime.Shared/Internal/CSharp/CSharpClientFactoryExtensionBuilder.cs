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

using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpClientFactoryExtensionBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;
        private readonly bool _isStaticClass;

        public CSharpClientFactoryExtensionBuilder(ContractDescription contract, bool isStaticClass)
        {
            _contract = contract;
            _isStaticClass = isStaticClass;
        }

        public override string GetGeneratedMemberName() => "Add" + _contract.ClientClassName;

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .Append("public static ")
                .AppendType(typeof(IClientFactory))
                .Append(" ")
                .Append(GetGeneratedMemberName())
                .Append("(");

            if (_isStaticClass)
            {
                Output.Append("this ");
            }

            Output
                .AppendType(typeof(IClientFactory))
                .Append(" clientFactory, Action<")
                .AppendType(typeof(ServiceModelGrpcClientOptions))
                .AppendLine("> configure = null)")
                .AppendLine("{");

            using (Output.Indent())
            {
                Output.AppendArgumentNullException("clientFactory");

                Output
                    .Append("clientFactory.")
                    .Append(nameof(IClientFactory.AddClient))
                    .Append("(new ")
                    .Append(_contract.ClientBuilderClassName)
                    .AppendLine("(), configure);");

                Output.AppendLine("return clientFactory;");
            }

            Output.AppendLine("}");
        }
    }
}
