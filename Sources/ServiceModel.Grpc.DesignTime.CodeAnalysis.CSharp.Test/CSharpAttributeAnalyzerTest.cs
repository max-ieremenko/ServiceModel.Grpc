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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.Generators.CSharp;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.CSharp;

[TestFixture]
public class CSharpAttributeAnalyzerTest
{
    [Test]
    [TestCaseSource(nameof(GetDoesLookLikeExpandableCases))]
    public void DoesLookLikeExpandable(SyntaxNode syntaxNode, bool expected)
    {
        CSharpAttributeAnalyzer.DoesLookLikeExpandable(syntaxNode).ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetDoesLookLikeExpandableCases()
    {
        var unit = (CompilationUnitSyntax)SyntaxFactory
            .ParseSyntaxTree("[ExportGrpcService] class A {}")
            .GetRoot();
        yield return new TestCaseData(unit.Members[0], true) { TestName = "export" };

        unit = (CompilationUnitSyntax)SyntaxFactory
            .ParseSyntaxTree("[ ImportGrpcService ] class A {}")
            .GetRoot();
        yield return new TestCaseData(unit.Members[0], true) { TestName = "import" };

        unit = (CompilationUnitSyntax)SyntaxFactory
            .ParseSyntaxTree("[ImportGrpcService, ExportGrpcService] class A {}")
            .GetRoot();
        yield return new TestCaseData(unit.Members[0], true) { TestName = "combined" };

        unit = (CompilationUnitSyntax)SyntaxFactory
            .ParseSyntaxTree("[ImportGrpcService] struct A {}")
            .GetRoot();
        yield return new TestCaseData(unit.Members[0], false) { TestName = "ignore struct" };

        unit = (CompilationUnitSyntax)SyntaxFactory
            .ParseSyntaxTree("[ServiceModelGrpcDesignTimeExtension] class A {}")
            .GetRoot();
        yield return new TestCaseData(unit.Members[0], true) { TestName = "extension" };
    }
}