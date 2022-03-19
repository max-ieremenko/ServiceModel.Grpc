// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    [Generator]
    internal sealed class ServiceModelGrpcSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SourceGeneratorSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!"C#".Equals(context.ParseOptions.Language, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var candidates = (context.SyntaxReceiver as SourceGeneratorSyntaxReceiver)?.Candidates;
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            using (var assemblyResolver = new AssemblyResolver(context.AnalyzerConfigOptions.GlobalOptions))
            {
                assemblyResolver.Initialize();

                var outputContext = new GeneratorContext(
                    context.Compilation,
                    context.ReportDiagnostic,
                    context.CancellationToken,
                    context.AddSource);
                new CSharpSourceGenerator().Execute(outputContext, candidates);
            }
        }
    }
}
