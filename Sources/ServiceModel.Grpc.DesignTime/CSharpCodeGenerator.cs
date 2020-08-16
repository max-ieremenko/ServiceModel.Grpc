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
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.Emit;
using ContractDescription = ServiceModel.Grpc.DesignTime.Internal.ContractDescription;
using ServiceContract = ServiceModel.Grpc.DesignTime.Internal.ServiceContract;

namespace ServiceModel.Grpc.DesignTime
{
    internal sealed class CSharpCodeGenerator : IRichCodeGenerator
    {
        private readonly INamedTypeSymbol _interfaceType;

        public CSharpCodeGenerator(AttributeData attributeData)
        {
            var interfaceType = (INamedTypeSymbol?)attributeData.ConstructorArguments[0].Value;
            if (interfaceType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(attributeData));
            }

            _interfaceType = interfaceType;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var node = (ClassDeclarationSyntax)context.ProcessingNode;

            if (!ServiceContract.IsServiceContractInterface(_interfaceType))
            {
                progress.Error(node, "{0} is not service contract.".FormatWith(_interfaceType.Name));
                return Task.FromResult(default(RichGenerationResult));
            }

            var contract = new ContractDescription(_interfaceType);

            var owner = SyntaxFactory
                .ClassDeclaration(node.Identifier.WithoutTrivia())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            var members = owner
                .AddMembers(
                    new CSharpClientBuilderBuilder(contract).AsMemberDeclaration(),
                    new CSharpContractBuilder(contract).AsMemberDeclaration(),
                    new CSharpClientBuilder(contract).AsMemberDeclaration())
                .AddMembers(GenerateMessages(contract).ToArray());

            return Task.FromResult(new RichGenerationResult
            {
                Members = CopyParent(members, node),
                Usings = BuildUsing(
                    context.CompilationUnitUsings.Select(i => i.Name.ToString()),
                    typeof(Func<>).Namespace,
                    typeof(IEnumerable<>).Namespace,
                    typeof(CancellationToken).Namespace,
                    typeof(Task).Namespace,
                    "Grpc.Core",
                    typeof(IMarshallerFactory).Namespace,
                    typeof(CallOptionsBuilder).Namespace,
                    typeof(DataContractAttribute).Namespace,
                    typeof(Message).Namespace)
            });
        }

        private static SyntaxList<MemberDeclarationSyntax> CopyParent(MemberDeclarationSyntax target, SyntaxNode source)
        {
            var result = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(target);
            foreach (var ancestor in source.Ancestors())
            {
                switch (ancestor)
                {
                    case NamespaceDeclarationSyntax ns:
                        result = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(SyntaxFactory.NamespaceDeclaration(ns.Name.WithoutTrivia()).WithMembers(result));
                        break;

                    case ClassDeclarationSyntax c:
                        result = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(SyntaxFactory.ClassDeclaration(c.Identifier.WithoutTrivia()).WithMembers(result));
                        break;
                }
            }

            return result;
        }

        private static SyntaxList<UsingDirectiveSyntax> BuildUsing(IEnumerable<string> existing, params string[] ns)
        {
            var distinct = new HashSet<string>(existing, StringComparer.Ordinal);

            var result = default(SyntaxList<UsingDirectiveSyntax>);
            var isInitialized = false;
            foreach (var name in ns.OrderBy(i => i, StringComparer.Ordinal))
            {
                if (!distinct.Add(name))
                {
                    continue;
                }

                var directive = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name));
                if (isInitialized)
                {
                    result = result.Add(directive);
                }
                else
                {
                    isInitialized = true;
                    result = SyntaxFactory.SingletonList(directive);
                }
            }

            return result;
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateMessages(ContractDescription contract)
        {
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
                    yield return new CSharpMessageBuilder(description).AsMemberDeclaration();
                }
            }
        }
    }
}
