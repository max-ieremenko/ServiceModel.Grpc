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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.DesignTime.Internal;
using ServiceModel.Grpc.DesignTime.Internal.CSharp;
using Logger = ServiceModel.Grpc.DesignTime.Internal.Logger;

namespace ServiceModel.Grpc.DesignTime
{
    internal abstract class CSharpCodeGeneratorBase : IRichCodeGenerator
    {
        private static readonly CodeGeneratorCache _generatorCache = new CodeGeneratorCache();

        protected CSharpCodeGeneratorBase(INamedTypeSymbol serviceType)
        {
            ServiceType = serviceType;
        }

        public INamedTypeSymbol ServiceType { get; }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var node = (ClassDeclarationSyntax)context.ProcessingNode;
            var logger = new Logger(progress, node, ServiceType);

            if (!Validate(logger))
            {
                return Task.FromResult(default(RichGenerationResult));
            }

            var contract = new ContractDescription(ServiceType);
            ShowWarnings(logger, contract);

            var owner = SyntaxFactory
                .ClassDeclaration(node.Identifier.WithoutTrivia())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            var declaration = owner;
            var generatedCount = 0;
            var imports = new HashSet<string>(StringComparer.Ordinal);
            foreach (var generator in GetGenerators(contract, node).Concat(GetSharedGenerators(contract)))
            {
                if (_generatorCache.AddNew(node, generator.GetGeneratedMemberName()))
                {
                    declaration = declaration.AddMembers(generator.AsMemberDeclaration());
                    foreach (var i in generator.GetUsing())
                    {
                        imports.Add(i);
                    }

                    generatedCount++;
                }
            }

            var result = default(RichGenerationResult);
            if (generatedCount > 0)
            {
                result.Members = declaration.CopyParent(node);
                result.Usings = BuildUsing(context.CompilationUnitUsings, GetSharedUsing().Concat(imports));
            }

            return Task.FromResult(result);
        }

        protected abstract bool Validate(Logger logger);

        protected abstract IEnumerable<CodeGeneratorBase> GetGenerators(ContractDescription contract, ClassDeclarationSyntax owner);

        private static IEnumerable<CodeGeneratorBase> GetSharedGenerators(ContractDescription contract)
        {
            yield return new CSharpContractBuilder(contract);

            var descriptions = contract
                .Services
                .SelectMany(i => i.Operations)
                .SelectMany(i => new[] { i.RequestType, i.ResponseType })
                .Where(i => !i.IsBuildIn)
                .OrderBy(i => i.Properties.Length);

            var distinct = new HashSet<int>();
            foreach (var description in descriptions)
            {
                if (distinct.Add(description.Properties.Length))
                {
                    yield return new CSharpMessageBuilder(description);
                }
            }
        }

        private static IEnumerable<string> GetSharedUsing()
        {
            yield return typeof(Func<>).Namespace;
            yield return typeof(IEnumerable<>).Namespace;
            yield return typeof(CancellationToken).Namespace;
            yield return typeof(Task).Namespace;
            yield return "Grpc.Core";
            yield return typeof(DataContractAttribute).Namespace;
            yield return typeof(IMarshallerFactory).Namespace;
            yield return typeof(Message).Namespace;
        }

        private static SyntaxList<UsingDirectiveSyntax> BuildUsing(
            IEnumerable<UsingDirectiveSyntax> existing,
            IEnumerable<string> ns)
        {
            var existingNames = existing
                .Where(i => i.Alias == null && i.StaticKeyword.Value == null)
                .Select(i => i.Name.ToString());

            var distinct = new HashSet<string>(existingNames, StringComparer.Ordinal);

            var result = default(SyntaxList<UsingDirectiveSyntax>);
            foreach (var name in ns.OrderBy(i => i, StringComparer.Ordinal))
            {
                if (!distinct.Add(name))
                {
                    continue;
                }

                var directive = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name));
                result = result.Add(directive);
            }

            return result;
        }

        private void ShowWarnings(Logger logger, ContractDescription contract)
        {
            foreach (var interfaceDescription in contract.Interfaces)
            {
                logger.WarnFormat("{0}: {1} is not service contract.", ServiceType.Name, interfaceDescription.InterfaceTypeName);
            }

            foreach (var interfaceDescription in contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    logger.WarnFormat("{0}: {1}", ServiceType.Name, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    logger.WarnFormat("{0}: {1}", ServiceType.Name, method.Error);
                }
            }
        }
    }
}
