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

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.DesignTime.Internal.CSharp;
using ServiceModel.Grpc.Internal;
using ContractDescription = ServiceModel.Grpc.DesignTime.Internal.ContractDescription;
using Logger = ServiceModel.Grpc.DesignTime.Internal.Logger;
using ServiceContract = ServiceModel.Grpc.DesignTime.Internal.ServiceContract;

namespace ServiceModel.Grpc.DesignTime
{
    internal sealed class CSharpClientCodeGenerator : CSharpCodeGeneratorBase
    {
        public CSharpClientCodeGenerator(AttributeData attributeData)
            : base(GetInterfaceType(attributeData))
        {
        }

        protected override bool Validate(Logger logger)
        {
            if (!ServiceContract.IsServiceContractInterface(ServiceType))
            {
                logger.ErrorFormat("{0} is not service contract.", ServiceType.Name);
                return false;
            }

            return true;
        }

        protected override IEnumerable<CodeGeneratorBase> GetGenerators(ContractDescription contract, ClassDeclarationSyntax owner)
        {
            yield return new CSharpClientFactoryExtensionBuilder(contract, owner.IsStatic());
            yield return new CSharpClientBuilderBuilder(contract);
            yield return new CSharpClientBuilder(contract);
        }

        private static INamedTypeSymbol GetInterfaceType(AttributeData attributeData)
        {
            var interfaceType = (INamedTypeSymbol?)attributeData.ConstructorArguments[0].Value;
            if (interfaceType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(attributeData));
            }

            return interfaceType;
        }
    }
}
