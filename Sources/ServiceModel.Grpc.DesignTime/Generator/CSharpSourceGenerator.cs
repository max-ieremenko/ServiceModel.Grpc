// <copyright>
// Copyright 2021-2022 Max Ieremenko
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.DesignTime.Generator.Internal;
using ServiceModel.Grpc.DesignTime.Generator.Internal.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal sealed class CSharpSourceGenerator
    {
        private readonly CodeGeneratorCache _generatorCache = new CodeGeneratorCache();

        public void Execute(GeneratorContext context, IList<ClassDeclarationSyntax> candidates)
        {
            foreach (var entry in ExpandCandidates(context, candidates))
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var logger = new Logger(context, entry.Candidate, entry.Attribute);
                InvokeGenerator(context, entry.Factory, entry.Candidate, logger);
            }
        }

        private static IEnumerable<(ClassDeclarationSyntax Candidate, AttributeData Attribute, ICodeGeneratorFactory Factory)> ExpandCandidates(
            GeneratorContext context,
            IList<ClassDeclarationSyntax> candidates)
        {
            foreach (var candidate in candidates.OrderBy(i => i.GetFullName(), StringComparer.Ordinal))
            {
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                var owner = model.GetDeclaredSymbol(candidate);
                if (owner != null)
                {
                    var attributes = owner.GetAttributes();
                    for (var j = 0; j < attributes.Length; j++)
                    {
                        var attribute = attributes[j];
                        if (TryResolveFactory(attribute, out var factory))
                        {
                            yield return (candidate, attribute, factory);
                        }
                    }
                }
            }
        }

        private static bool TryResolveFactory(AttributeData attribute, out ICodeGeneratorFactory factory)
        {
            if (CSharpClientCodeGeneratorFactory.Math(attribute))
            {
                factory = new CSharpClientCodeGeneratorFactory(attribute);
                return true;
            }

            if (CSharpServiceCodeGeneratorFactory.Math(attribute))
            {
                factory = new CSharpServiceCodeGeneratorFactory(attribute);
                return true;
            }

            factory = null!;
            return false;
        }

        private static IEnumerable<CodeGeneratorBase> GetSharedGenerators(ContractDescription contract)
        {
            yield return new CSharpContractBuilder(contract);

            var descriptions = contract
                .Services
                .SelectMany(i => i.Operations)
                .SelectMany(i => new[] { i.RequestType, i.ResponseType, i.HeaderRequestType, i.HeaderResponseType })
                .Where(i => i != null && !i.IsBuildIn)
                .OrderBy(i => i!.Properties.Length);

            var distinct = new HashSet<int>();
            foreach (var description in descriptions)
            {
                if (distinct.Add(description!.Properties.Length))
                {
                    yield return new CSharpMessageBuilder(description);
                }
            }
        }

        private static IEnumerable<string> GetSharedUsing()
        {
            yield return typeof(Func<>).Namespace!;
            yield return typeof(IEnumerable<>).Namespace!;
            yield return typeof(CancellationToken).Namespace!;
            yield return typeof(Task).Namespace!;
            yield return "Grpc.Core";
            yield return typeof(DataContractAttribute).Namespace!;
            yield return typeof(IMarshallerFactory).Namespace!;
            yield return typeof(Message).Namespace!;
            yield return typeof(CompilerGeneratedAttribute).Namespace!;
            yield return typeof(GeneratedCodeAttribute).Namespace!;
            yield return typeof(ExcludeFromCodeCoverageAttribute).Namespace!;
            yield return typeof(ObfuscationAttribute).Namespace!;
        }

        private static void ShowWarnings(Logger logger, ContractDescription contract, INamedTypeSymbol serviceType)
        {
            foreach (var interfaceDescription in contract.Interfaces)
            {
                logger.WarnFormat("{0}: {1} is not service contract.", serviceType.Name, interfaceDescription.InterfaceTypeName);
            }

            foreach (var interfaceDescription in contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    logger.WarnFormat("{0}: {1}", serviceType.Name, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    logger.WarnFormat("{0}: {1}", serviceType.Name, method.Error);
                }
            }
        }

        private void InvokeGenerator(
            GeneratorContext context,
            ICodeGeneratorFactory factory,
            ClassDeclarationSyntax node,
            Logger logger)
        {
            if (!factory.Validate(logger))
            {
                return;
            }

            var contract = new ContractDescription(factory.ServiceType);
            ShowWarnings(logger, contract, factory.ServiceType);

            var unit = new CompilationUnit(node);

            var generatedCount = 0;
            var imports = new HashSet<string>(StringComparer.Ordinal);
            foreach (var generator in factory.GetGenerators(contract, node).Concat(GetSharedGenerators(contract)))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (_generatorCache.AddNew(node, generator.GetGeneratedMemberName()))
                {
                    if (generatedCount > 0)
                    {
                        unit.Output.AppendLine();
                    }

                    generator.GenerateMemberDeclaration(unit.Output);

                    foreach (var i in generator.GetUsing())
                    {
                        imports.Add(i);
                    }

                    generatedCount++;
                }
            }

            context.CancellationToken.ThrowIfCancellationRequested();
            if (generatedCount > 0)
            {
                var source = unit.GetSourceText(GetSharedUsing().Concat(imports));
                context.AddOutput(node, factory.GetHintName(contract), source);
            }
        }
    }
}
