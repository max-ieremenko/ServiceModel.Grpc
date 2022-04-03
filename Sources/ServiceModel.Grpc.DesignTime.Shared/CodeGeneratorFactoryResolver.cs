// <copyright>
// Copyright 2022 Max Ieremenko
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
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.DesignTime.Generator.Internal;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal readonly ref struct CodeGeneratorFactoryResolver
    {
        private readonly GeneratorContext _context;
        private readonly ClassDeclarationSyntax _candidate;
        private readonly bool _canUseStaticExtensions;

        public CodeGeneratorFactoryResolver(GeneratorContext context, ClassDeclarationSyntax candidate)
        {
            _context = context;
            _candidate = candidate;
            _canUseStaticExtensions = candidate.IsStatic();
        }

        public bool TryResolve(
            AttributeData attributeData,
            [NotNullWhen(true)] out ICodeGeneratorFactory? factory,
            [NotNullWhen(true)] out ContractDescription? contract)
        {
            factory = null;
            contract = null;

            if (attributeData.AttributeClass == null
                || attributeData.ConstructorArguments.Length != 1
                || attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol serviceType)
            {
                return false;
            }

            var fullName = attributeData.AttributeClass.ToDisplayString(NullableFlowState.None);
            var isImport = false;
            if ("ServiceModel.Grpc.DesignTime.ImportGrpcServiceAttribute".Equals(fullName, StringComparison.Ordinal))
            {
                isImport = true;
            }
            else if (!"ServiceModel.Grpc.DesignTime.ExportGrpcServiceAttribute".Equals(fullName, StringComparison.Ordinal))
            {
                return false;
            }

            var logger = new Logger(_context, _candidate, attributeData);
            if (isImport && !ServiceContract.IsServiceContractInterface(serviceType))
            {
                logger.IsNotServiceContract(serviceType);
                return false;
            }

            contract = new ContractDescription(serviceType);
            ShowCommonWarnings(logger, contract, serviceType);

            factory = isImport ? new CSharpClientCodeGeneratorFactory(contract, _canUseStaticExtensions) : CreateServiceCodeFactory(contract, attributeData);
            return true;
        }

        private static void ShowCommonWarnings(in Logger logger, ContractDescription contract, INamedTypeSymbol serviceType)
        {
            foreach (var interfaceDescription in contract.Interfaces)
            {
                logger.InheritsNotServiceContract(serviceType, interfaceDescription.InterfaceType);
            }

            foreach (var interfaceDescription in contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    logger.IsNotOperationContract(serviceType, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    logger.IsNotSupportedOperation(serviceType, method.Error);
                }
            }
        }

        private CSharpServiceCodeGeneratorFactory CreateServiceCodeFactory(ContractDescription contract, AttributeData attributeData)
        {
            var generateAspNetExtensions = false;
            var generateSelfHostExtensions = false;

            for (var i = 0; i < attributeData.NamedArguments.Length; i++)
            {
                var arg = attributeData.NamedArguments[i];
                if ("GenerateAspNetExtensions".Equals(arg.Key, StringComparison.Ordinal))
                {
                    if (arg.Value.Value is bool flag)
                    {
                        generateAspNetExtensions = flag;
                    }
                }
                else if ("GenerateSelfHostExtensions".Equals(arg.Key, StringComparison.Ordinal))
                {
                    if (arg.Value.Value is bool flag)
                    {
                        generateSelfHostExtensions = flag;
                    }
                }
            }

            return new CSharpServiceCodeGeneratorFactory(
                contract,
                generateAspNetExtensions,
                generateSelfHostExtensions,
                _canUseStaticExtensions);
        }
    }
}
