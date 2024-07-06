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

using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

[TestFixture]
public partial class AttributeAnalyzerTest
{
    private readonly CSharpCompilation _compilation = CSharpCompilationExtensions.CreateDefault();

    private readonly TypeHandler _typeHandler = new(typeof(ImportGrpcServiceExtension), typeof(ExportGrpcServiceExtension), null!);

    [Test]
    public void ResolveCustomExtension()
    {
        var placeHolder = _compilation.ResolveTypeSymbol(typeof(ExtensionHolder));

        var attributes = placeHolder.GetAttributes();
        attributes.Length.ShouldBe(1);

        var attribute = attributes[0];

        AttributeAnalyzer.TryGetProviderType(_typeHandler, attribute, out var actual, out _).ShouldBeTrue();

        actual.ShouldBe(_compilation.ResolveTypeSymbol(typeof(SomeExtension)));
    }

    [Test]
    public void ResolveExportGrpcService()
    {
        var placeHolder = _compilation.ResolveTypeSymbol(typeof(ExportHolder));

        var attributes = placeHolder.GetAttributes();
        attributes.Length.ShouldBe(1);

        AttributeAnalyzer.TryGetProviderType(_typeHandler, attributes[0], out _, out var actual).ShouldBeTrue();

        actual.ShouldBe(typeof(ExportGrpcServiceExtension));
    }

    [Test]
    public void ResolveImportGrpcService()
    {
        var placeHolder = _compilation.ResolveTypeSymbol(typeof(ImportHolder));

        var attributes = placeHolder.GetAttributes();
        attributes.Length.ShouldBe(1);

        AttributeAnalyzer.TryGetProviderType(_typeHandler, attributes[0], out _, out var actual).ShouldBeTrue();

        actual.ShouldBe(typeof(ImportGrpcServiceExtension));
    }
}