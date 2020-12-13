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
    internal sealed class CSharpServiceCodeGeneratorFactory : ICodeGeneratorFactory
    {
        private readonly bool _generateAspNetExtensions;
        private readonly bool _generateSelfHostExtensions;

        public CSharpServiceCodeGeneratorFactory(AttributeData attributeData)
        {
            var serviceType = (INamedTypeSymbol?)attributeData.ConstructorArguments[0].Value;
            if (serviceType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(attributeData));
            }

            ServiceType = serviceType;
            for (var i = 0; i < attributeData.NamedArguments.Length; i++)
            {
                var arg = attributeData.NamedArguments[i];
                if (string.Equals("GenerateAspNetExtensions", arg.Key, StringComparison.Ordinal))
                {
                    if (arg.Value.Value is bool flag)
                    {
                        _generateAspNetExtensions = flag;
                    }
                }
                else if (string.Equals("GenerateSelfHostExtensions", arg.Key, StringComparison.Ordinal))
                {
                    if (arg.Value.Value is bool flag)
                    {
                        _generateSelfHostExtensions = flag;
                    }
                }
            }
        }

        public INamedTypeSymbol ServiceType { get; }

        public static bool Math(AttributeData attributeData)
        {
            return attributeData.AttributeClass != null
                   && "ServiceModel.Grpc.DesignTime.ExportGrpcServiceAttribute".Equals(SyntaxTools.GetFullName(attributeData.AttributeClass), StringComparison.Ordinal);
        }

        public bool Validate(Logger logger)
        {
            return true;
        }

        public IEnumerable<CodeGeneratorBase> GetGenerators(ContractDescription contract, ClassDeclarationSyntax owner)
        {
            if (_generateAspNetExtensions)
            {
                yield return new CSharpServiceAspNetAddOptionsBuilder(contract, owner.IsStatic());
                yield return new CSharpServiceAspNetMapGrpcServiceBuilder(contract, owner.IsStatic());
            }

            if (_generateSelfHostExtensions)
            {
                yield return new CSharpServiceSelfHostAddSingletonServiceBuilder(contract, owner.IsStatic());
                yield return new CSharpServiceSelfHostAddTransientServiceBuilder(contract, owner.IsStatic());
            }

            yield return new CSharpServiceEndpointBuilder(contract);
            yield return new CSharpServiceEndpointBinderBuilder(contract);
        }

        public string GetHintName(ContractDescription contract) => contract.EndpointClassName;
    }
}
