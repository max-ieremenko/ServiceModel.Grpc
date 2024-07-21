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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.TestApi;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

[TestFixture]
public partial class CodeStringBuilderExtensionsTest
{
    private static readonly CSharpCompilation Compilation = CSharpCompilationExtensions.CreateDefault();

    private CodeStringBuilder _output = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new();
    }

    [Test]
    [TestCaseSource(nameof(GetWriteTypeCases))]
    public void WriteType(ITypeSymbol type, string expected)
    {
        _output.WriteType(type);

        _output.Clear().ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetWriteTypeCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(WriteTypeCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var attribute = SyntaxTools.GetCustomAttribute(method.GetAttributes(), typeof(DisplayNameAttribute).FullName!);
            var expected = attribute!.ConstructorArguments[0].Value.ShouldBeOfType<string>();

            yield return new TestCaseData(method.ReturnType, expected) { TestName = "AppendType." + method.Name };
        }
    }
}