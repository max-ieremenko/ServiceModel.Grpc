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
using Grpc.Core;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Client.Internal;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp
{
    internal sealed class CSharpClientBuilderBuilder : CodeGeneratorBase
    {
        private readonly ContractDescription _contract;

        public CSharpClientBuilderBuilder(ContractDescription contract)
        {
            _contract = contract;
        }

        public override string GetGeneratedMemberName() => _contract.ClientBuilderClassName;

        protected override void Generate()
        {
            WriteMetadata();
            Output
                .Append("public sealed class ")
                .Append(_contract.ClientBuilderClassName)
                .Append(" : ")
                .AppendType(typeof(IClientBuilder<>))
                .Append(_contract.ContractInterfaceName)
                .AppendLine(">");
            Output.AppendLine("{");

            using (Output.Indent())
            {
                BuildFields();
                Output.AppendLine();

                BuildCtor();
                Output.AppendLine();

                BuildMethodInitialize();
                Output.AppendLine();

                BuildMethodBuild();
            }

            Output.AppendLine("}");
        }

        private void BuildCtor()
        {
            Output
                .Append("public ")
                .Append(_contract.ClientBuilderClassName)
                .AppendLine("()")
                .AppendLine("{")
                .AppendLine("}");
        }

        private void BuildFields()
        {
            Output
                .Append("private ")
                .Append(_contract.ContractClassName)
                .AppendLine(" _contract;");

            Output
                .AppendLine("private Func<")
                .AppendType(typeof(CallOptions))
                .AppendLine("> _defaultCallOptionsFactory;");
        }

        private void BuildMethodInitialize()
        {
            Output
                .Append("public void Initialize(")
                .AppendType(typeof(IMarshallerFactory))
                .Append(" marshallerFactory, Func<")
                .AppendType(typeof(CallOptions))
                .AppendLine("> defaultCallOptionsFactory)");

            Output.AppendLine("{");
            using (Output.Indent())
            {
                Output
                    .AppendArgumentNullException("marshallerFactory");

                Output
                    .Append("_contract = new ")
                    .Append(_contract.ContractClassName)
                    .AppendLine("(marshallerFactory);");

                Output
                    .AppendLine("_defaultCallOptionsFactory = defaultCallOptionsFactory;");
            }

            Output.AppendLine("}");
        }

        private void BuildMethodBuild()
        {
            Output
                .Append("public ")
                .Append(_contract.ContractInterfaceName)
                .Append(" Build(")
                .AppendType(typeof(CallInvoker))
                .AppendLine(" callInvoker)");

            Output.AppendLine("{");
            using (Output.Indent())
            {
                Output
                    .AppendArgumentNullException("callInvoker");

                Output
                    .Append("return new ")
                    .Append(_contract.ClientClassName)
                    .AppendLine("(callInvoker, _contract, _defaultCallOptionsFactory);");
            }

            Output.AppendLine("}");
        }
    }
}
