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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.DesignTime.Generator
{
    internal sealed class CSharpSourceGenerator
    {
        private readonly CodeGeneratorCache _generatorCache = new CodeGeneratorCache();

        public void Execute(GeneratorContext context, IEnumerable<ClassDeclarationSyntax> candidates)
        {
            foreach (var candidate in candidates.OrderBy(i => i.GetFullName(), StringComparer.Ordinal))
            {
                var owner = context.Compilation.GetSemanticModel(candidate.SyntaxTree).GetDeclaredSymbol(candidate);
                if (owner == null)
                {
                    continue;
                }

                var factoryResolver = new CodeGeneratorFactoryResolver(context, candidate);
                var attributes = owner.GetAttributes();

                for (var i = 0; i < attributes.Length; i++)
                {
                    if (factoryResolver.TryResolve(attributes[i], out var factory, out var contract))
                    {
                        InvokeGenerator(context, factory, candidate);
                        InvokeGenerator(context, new CSharpSharedCodeGeneratorFactory(contract), candidate);
                    }
                }
            }
        }

        private static ICollection<string> CreateDefaultUsing()
        {
            return new HashSet<string>(StringComparer.Ordinal)
            {
                typeof(Func<>).Namespace,
                typeof(IEnumerable<>).Namespace,
                typeof(CancellationToken).Namespace,
                typeof(Task).Namespace,
                "Grpc.Core",
                typeof(IMarshallerFactory).Namespace,
                typeof(Message).Namespace
            };
        }

        private void InvokeGenerator(GeneratorContext context, ICodeGeneratorFactory factory, ClassDeclarationSyntax node)
        {
            var generatedCount = 0;

            CompilationUnit unit = default;
            ICollection<string> imports = null!;

            foreach (var generator in factory.GetGenerators())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (_generatorCache.AddNew(node, generator.GetGeneratedMemberName()))
                {
                    if (generatedCount == 0)
                    {
                        unit = new CompilationUnit(node);
                        imports = CreateDefaultUsing();
                    }
                    else
                    {
                        unit.Output.AppendLine();
                    }

                    generator.AddUsing(imports);
                    generator.GenerateMemberDeclaration(unit.Output);

                    generatedCount++;
                }
            }

            context.CancellationToken.ThrowIfCancellationRequested();
            if (generatedCount > 0)
            {
                var source = unit.GetSourceText(imports);
                context.AddOutput(node, factory.GetHintName(), source);
            }
        }
    }
}
