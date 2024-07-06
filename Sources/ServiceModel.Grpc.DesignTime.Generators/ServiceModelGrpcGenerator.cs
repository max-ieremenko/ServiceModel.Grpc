// <copyright>
// Copyright Max Ieremenko
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
using Microsoft.CodeAnalysis.Diagnostics;
using ServiceModel.Grpc.DesignTime.Generators.CSharp;

namespace ServiceModel.Grpc.DesignTime.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceModelGrpcGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidatesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (syntaxNode, _) => SyntaxProviderPredicate.DoesLookLikeExpandable(syntaxNode),
            (syntaxContext, _) => syntaxContext.Node);

        var optionsProvider = context.AnalyzerConfigOptionsProvider.Select((provider, _) => provider.GlobalOptions);

        var compilationProvider = context.CompilationProvider.Select((compilation, _) => compilation);

        var source = candidatesProvider
            .Collect()
            .Combine(optionsProvider)
            .Combine(compilationProvider)
            .Select((i, _) => (i.Left.Left, i.Left.Right, i.Right));

        context.RegisterSourceOutput(source, Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        (ImmutableArray<SyntaxNode> Candidates, AnalyzerConfigOptions GlobalOptions, Compilation Compilation) source)
    {
        if (source.Candidates.Length == 0 || !LanguageNames.CSharp.Equals(source.Compilation.Language, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var logger = DebugLogger.Create(source.GlobalOptions);
        try
        {
            using (var assemblyResolver = new AssemblyResolver(source.GlobalOptions, logger))
            {
                CSharpProvider.Execute(context, assemblyResolver, logger, source);
            }
        }
        catch (Exception ex) when (!context.CancellationToken.IsCancellationRequested)
        {
            logger?.Log($"Execute error: {ex}");
            throw;
        }
    }
}