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
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.DesignTime.Internal.CSharp;
using ServiceModel.Grpc.Internal;
using ContractDescription = ServiceModel.Grpc.DesignTime.Internal.ContractDescription;
using Logger = ServiceModel.Grpc.DesignTime.Internal.Logger;
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
            var logger = new Logger(progress, node, _interfaceType);

            if (!ServiceContract.IsServiceContractInterface(_interfaceType))
            {
                logger.ErrorFormat("{0} is not service contract.", _interfaceType.Name);
                return Task.FromResult(default(RichGenerationResult));
            }

            var contract = new ContractDescription(_interfaceType);

            ShowWarnings(logger, contract);

            var owner = SyntaxFactory
                .ClassDeclaration(node.Identifier.WithoutTrivia())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            var members = owner.AddMembers(
                new CSharpClientFactoryExtensionBuilder(contract, node.IsStatic()).AsMemberDeclaration(),
                new CSharpClientBuilderBuilder(contract).AsMemberDeclaration(),
                new CSharpContractBuilder(contract).AsMemberDeclaration(),
                new CSharpClientBuilder(contract).AsMemberDeclaration());

            var messageFlags = new List<UsingDirectiveSyntax>();
            foreach (var entry in GenerateMessages(contract, context.CompilationUnitUsings, node.GetFullName()))
            {
                members = members.AddMembers(entry.Member);
                messageFlags.Add(entry.Flag);
            }

            return Task.FromResult(new RichGenerationResult
            {
                Members = members.CopyParent(node),
                Usings = BuildUsing(
                    context.CompilationUnitUsings,
                    messageFlags,
                    typeof(Func<>).Namespace,
                    typeof(IEnumerable<>).Namespace,
                    typeof(CancellationToken).Namespace,
                    typeof(Task).Namespace,
                    "Grpc.Core",
                    typeof(IClientFactory).Namespace,
                    typeof(IMarshallerFactory).Namespace,
                    typeof(CallOptionsBuilder).Namespace,
                    typeof(DataContractAttribute).Namespace,
                    typeof(Message).Namespace)
            });
        }

        private static SyntaxList<UsingDirectiveSyntax> BuildUsing(
            IEnumerable<UsingDirectiveSyntax> existing,
            IList<UsingDirectiveSyntax> messageFlags,
            params string[] ns)
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

            result = result.AddRange(messageFlags);

            return result;
        }

        private static IEnumerable<(MemberDeclarationSyntax Member, UsingDirectiveSyntax Flag)> GenerateMessages(
            ContractDescription contract,
            IEnumerable<UsingDirectiveSyntax> directives,
            string ownerFullName)
        {
            var descriptions = contract
                .Services
                .SelectMany(i => i.Operations)
                .SelectMany(i => new[] { i.RequestType, i.ResponseType })
                .Where(i => !i.IsBuildIn)
                .Where(i => !CSharpMessageBuilder.ContainsFlag(ownerFullName, directives, i))
                .OrderBy(i => i.Properties.Length);

            var distinct = new HashSet<int>();
            foreach (var description in descriptions)
            {
                if (distinct.Add(description.Properties.Length))
                {
                    var builder = new CSharpMessageBuilder(description);
                    yield return (builder.AsMemberDeclaration(), builder.CreateFlag(ownerFullName));
                }
            }
        }

        private void ShowWarnings(Logger logger, ContractDescription contract)
        {
            foreach (var interfaceDescription in contract.Interfaces)
            {
                logger.WarnFormat("{0}: {1} is not service contract.", _interfaceType.Name, interfaceDescription.InterfaceTypeName);
            }

            foreach (var interfaceDescription in contract.Services)
            {
                foreach (var method in interfaceDescription.Methods)
                {
                    logger.WarnFormat("{0}: {1}", _interfaceType.Name, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    logger.WarnFormat("{0}: {1}", _interfaceType.Name, method.Error);
                }
            }
        }
    }
}
