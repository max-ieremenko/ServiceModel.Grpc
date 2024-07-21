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
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.TestApi;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

[TestFixture]
public partial class CompilationUnitTest
{
    private readonly CSharpCompilation _compilation = CSharpCompilationExtensions.CreateDefault();
    private readonly CompilationUnit _sut = new();
    private readonly CodeStringBuilder _output = new();

    [TearDown]
    public void AfterEachTest()
    {
        _output.Clear();
    }

    [Test]
    [TestCaseSource(nameof(GetGenerateHolderTestCases))]
    public void GenerateHolderTest(Type holderType, string[] expected)
    {
        var holder = _compilation.ResolveTypeSymbol(holderType);

        _sut.BeginDeclaration(_output, holder);
        _sut.EndDeclaration(_output);

        var actual = _output.Clear();
        TestOutput.WriteLine(actual);

        var lines = actual
            .Split(['\r', '\n', '{', '}'])
            .Select(i => i.Trim())
            .Where(i => i.Length > 0)
            .ToArray();
        lines.ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetGenerateHolderTestCases()
    {
        yield return new(
            typeof(CompilationUnitTest),
            new[]
            {
                "namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators",
                $"partial class {nameof(CompilationUnitTest)}"
            }) { TestName = "ns.class" };

        yield return new(
            typeof(NestedHolder),
            new[]
            {
                "namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators",
                $"partial class {nameof(CompilationUnitTest)}",
                $"static partial class {nameof(NestedHolder)}"
            }) { TestName = "ns.class1.class2" };

        yield return new(
                typeof(NestedHolder.ChildHolder),
                new[]
                {
                    "namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators",
                    $"partial class {nameof(CompilationUnitTest)}",
                    $"static partial class {nameof(NestedHolder)}",
                    $"partial class {nameof(NestedHolder.ChildHolder)}"
                })
            { TestName = "ns.class1.class2.class3" };

        yield return new(
            typeof(CompilationUnitTestGlobal),
            new[]
            {
                $"static partial class {nameof(CompilationUnitTestGlobal)}"
            }) { TestName = "class" };

        yield return new(
                typeof(CompilationUnitTestGlobal.NestedHolder),
                new[]
                {
                    $"static partial class {nameof(CompilationUnitTestGlobal)}",
                    $"static partial class {nameof(CompilationUnitTestGlobal.NestedHolder)}"
                })
            { TestName = "class1.class2" };

        yield return new(
                typeof(CompilationUnitTestGlobal.NestedHolder.ChildHolder),
                new[]
                {
                    $"static partial class {nameof(CompilationUnitTestGlobal)}",
                    $"static partial class {nameof(CompilationUnitTestGlobal.NestedHolder)}",
                    $"partial class {nameof(CompilationUnitTestGlobal.NestedHolder.ChildHolder)}"
                })
            { TestName = "class1.class2.class3" };
    }
}