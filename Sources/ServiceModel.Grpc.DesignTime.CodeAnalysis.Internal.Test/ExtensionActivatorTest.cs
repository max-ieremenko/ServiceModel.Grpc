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

using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

[TestFixture]
public partial class ExtensionActivatorTest
{
    private static readonly CSharpCompilation Compilation = CreateCompilation();

    [Test]
    public void ResolveTypeSymbolAssemblyPath_SourceFile()
    {
        var typeSymbol = Compilation.ResolveTypeSymbol(typeof(SomeExtension));

        typeSymbol.Locations.Length.ShouldBe(1);
        typeSymbol.Locations[0].Kind.ShouldBe(LocationKind.SourceFile);

        Should.Throw<NotSupportedException>(() => ExtensionActivator.ResolveAssemblyPath(typeSymbol, Compilation));
    }

    [Test]
    public void ResolveTypeSymbolAssemblyPath_ProjectReference()
    {
        var typeSymbol = Compilation.ResolveTypeSymbol(typeof(ExtensionsHandler));

        typeSymbol.Locations.Length.ShouldBe(1);
        typeSymbol.Locations[0].Kind.ShouldBe(LocationKind.MetadataFile);

        var actual = ExtensionActivator.ResolveAssemblyPath(typeSymbol, Compilation);
        File.Exists(actual).ShouldBeTrue();
    }

    [Test]
    public void ResolveTypeSymbolAssemblyPath_ExternalReference()
    {
        var typeSymbol = Compilation.ResolveTypeSymbol(typeof(object));

        typeSymbol.Locations.Length.ShouldBe(1);
        typeSymbol.Locations[0].Kind.ShouldBe(LocationKind.MetadataFile);

        var actual = ExtensionActivator.ResolveAssemblyPath(typeSymbol, Compilation);
        File.Exists(actual).ShouldBeTrue();
    }

    private static CSharpCompilation CreateCompilation()
    {
        // bin/Debug/net8.0
        var fileName = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ExtensionActivatorTest.Domain.cs");
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(fileName));

        return CSharpCompilationExtensions.Create(syntaxTree);
    }
}