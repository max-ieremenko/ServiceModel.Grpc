// <copyright>
// Copyright 2024 Max Ieremenko
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
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ServiceModel.Grpc.DesignTime.CodeAnalysis;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.Extensions;

namespace ServiceModel.Grpc.DesignTime.Generators.CSharp;

internal static class CSharpProvider
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Execute(
        SourceProductionContext context,
        AssemblyResolver assemblyResolver,
        DebugLogger? logger,
        in (ImmutableArray<SyntaxNode> Candidates, AnalyzerConfigOptions GlobalOptions, Compilation Compilation) source)
    {
        var delegatingLogger = logger == null ? null : new DelegatingDebugLogger(logger.Log, logger.LogSource);
        var handler = new ExtensionsHandler(
            context,
            delegatingLogger,
            source.Compilation,
            new TypeHandler(typeof(ImportGrpcService), typeof(ExportGrpcService), assemblyResolver.Resolve),
            new CompilationUnit());
        handler.Execute(source.Candidates);
    }
}