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

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CodeGenerators;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.TestApi;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp.CodeGenerators;

[TestFixture]
public partial class EndpointBinderCodeGeneratorTest
{
    private static readonly CSharpCompilation Compilation = CSharpCompilationExtensions.CreateDefault();

    private CodeStringBuilder _output = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new();
    }

    [Test]
    [TestCaseSource(nameof(GetWriteNewAttributeCases))]
    public void WriteNewAttribute(AttributeData attribute, string expected)
    {
        EndpointBinderCodeGenerator.WriteNewAttribute(_output, attribute);

        _output.Clear().ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetWriteNewAttributeCases()
    {
        var type = typeof(WriteNewAttributeCases);
        var symbol = Compilation.ResolveTypeSymbol(type);

        foreach (var method in SyntaxTools.GetInstanceMethods(symbol))
        {
            var attributes = EndpointBinderCodeGenerator.FilterAttributes(method.GetAttributes()).ToList();
            attributes.Count.ShouldBe(1);

            var expected = (string)type
                .GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                ?.Invoke(new WriteNewAttributeCases(), [])!;

            yield return new TestCaseData(attributes[0], expected)
            {
                TestName = "WriteAttribute." + method.Name
            };
        }
    }
}