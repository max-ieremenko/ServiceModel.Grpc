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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    [Generator]
    internal sealed class ServiceModelGrpcSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidatesProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) => SourceGeneratorSyntaxReceiver.IsCandidate(syntaxNode),
                (syntaxContext, _) => (ClassDeclarationSyntax)syntaxContext.Node);

            var assemblyResolverProvider = context.AnalyzerConfigOptionsProvider.Select((provider, _) => new AssemblyResolver(provider.GlobalOptions));

            var compilationProvider = context.CompilationProvider.Select((compilation, _) => compilation);

            var source = candidatesProvider
                .Collect()
                .Combine(assemblyResolverProvider)
                .Combine(compilationProvider)
                .Select((i, _) => (i.Left.Left, i.Left.Right, i.Right));

            context.RegisterSourceOutput(source, Execute);
        }

        private static void Execute(
            SourceProductionContext context,
            (ImmutableArray<ClassDeclarationSyntax> Candidates, AssemblyResolver AssemblyResolver, Compilation Compilation) source)
        {
            using (source.AssemblyResolver)
            {
                if (source.Candidates.Length > 0)
                {
                    source.AssemblyResolver.Initialize();

                    var outputContext = new GeneratorContext(
                        source.Compilation,
                        context.ReportDiagnostic,
                        context.CancellationToken,
                        context.AddSource);
                    new CSharpSourceGenerator().Execute(outputContext, source.Candidates);
                }
            }
        }
    }
}
