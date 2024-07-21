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

using System.ComponentModel;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.TestApi;

internal static class CSharpCompilationExtensions
{
    private static CSharpCompilation? _defaultCompilation;

    public static CSharpCompilation CreateDefault() => _defaultCompilation ??= CreateCompilation();

    public static INamedTypeSymbol ResolveTypeSymbol(this CSharpCompilation compilation, Type type)
        => ResolveTypeSymbolCore(compilation, type).ShouldBeAssignableTo<INamedTypeSymbol>()!;

    private static ITypeSymbol ResolveTypeSymbolCore(CSharpCompilation compilation, Type metadata)
    {
        ITypeSymbol? symbol;
        if (metadata.IsGenericType && !metadata.IsGenericTypeDefinition)
        {
            var unbound = compilation.ResolveTypeSymbol(metadata.GetGenericTypeDefinition());
            var unboundArgs = metadata.GetGenericArguments();

            var args = new ITypeSymbol[unboundArgs.Length];
            for (var i = 0; i < unboundArgs.Length; i++)
            {
                args[i] = ResolveTypeSymbolCore(compilation, unboundArgs[i]);
            }

            symbol = unbound.Construct(args);
        }
        else if (metadata.IsArray)
        {
            var unbound = metadata.GetElementType()!;
            var elementType = ResolveTypeSymbolCore(compilation, unbound);
            symbol = compilation.CreateArrayTypeSymbol(elementType, metadata.GetArrayRank());
        }
        else
        {
            symbol = compilation.GetTypeByMetadataName(metadata.FullName!);
        }

        symbol.ShouldNotBeNull();
        return symbol;
    }

    private static CSharpCompilation CreateCompilation(SyntaxTree? syntaxTree = null)
    {
        var owner = typeof(CSharpCompilationExtensions);

        var referencedAssemblies = owner
            .Assembly
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Select(i => i.Location);

        var assemblies = new HashSet<string>(referencedAssemblies, StringComparer.Ordinal)
        {
            typeof(object).Assembly.Location,
            typeof(string).Assembly.Location,
            typeof(IDisposable).Assembly.Location,
            typeof(Task).Assembly.Location,
            typeof(DisplayNameAttribute).Assembly.Location
        };

        if (syntaxTree == null)
        {
            assemblies.Add(owner.Assembly.Location);
        }

        var references = assemblies.Select(i => MetadataReference.CreateFromFile(i)).ToArray();

        var compilation = CSharpCompilation
            .Create(
                owner.FullName,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true),
                references: references,
                syntaxTrees: syntaxTree == null ? Enumerable.Empty<SyntaxTree>() : [syntaxTree]);

        compilation.GetDiagnostics().Where(i => i.Severity > DiagnosticSeverity.Warning).ShouldBeEmpty();
        compilation.GetParseDiagnostics().ShouldBeEmpty();

        return compilation;
    }
}