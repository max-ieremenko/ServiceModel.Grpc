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
using ServiceModel.Grpc.DesignTime.Generator.Internal;
using ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal sealed class CSharpClientCodeGeneratorFactory : ICodeGeneratorFactory
    {
        public CSharpClientCodeGeneratorFactory(AttributeData attributeData)
        {
            var interfaceType = (INamedTypeSymbol?)attributeData.ConstructorArguments[0].Value;
            if (interfaceType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(attributeData));
            }

            ServiceType = interfaceType;
        }

        public INamedTypeSymbol ServiceType { get; }

        public static bool Math(AttributeData attributeData)
        {
            return attributeData.AttributeClass != null
                   && "ServiceModel.Grpc.DesignTime.ImportGrpcServiceAttribute".Equals(SyntaxTools.GetFullName(attributeData.AttributeClass), StringComparison.Ordinal);
        }

        public bool Validate(Logger logger)
        {
            if (!ServiceContract.IsServiceContractInterface(ServiceType))
            {
                logger.ErrorFormat("{0} is not service contract.", ServiceType.Name);
                return false;
            }

            return true;
        }

        public IEnumerable<CodeGeneratorBase> GetGenerators(ContractDescription contract, ClassDeclarationSyntax owner)
        {
            yield return new CSharpClientFactoryExtensionBuilder(contract, owner.IsStatic());
            yield return new CSharpClientBuilderBuilder(contract);
            yield return new CSharpClientBuilder(contract);
        }

        public string GetHintName(ContractDescription contract) => contract.ClientClassName;
    }
}
