// <copyright>
// Copyright 2020-2024 Max Ieremenko
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
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis;

[TestFixture]
public partial class SyntaxToolsTest
{
    private static readonly CSharpCompilation Compilation = CSharpCompilationExtensions.CreateDefault();

    [Test]
    [TestCase(typeof(I1), true)]
    [TestCase(typeof(SyntaxToolsTest), false)]
    public void IsInterface(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        SyntaxTools.IsInterface(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(I1), typeof(ServiceContractAttribute), true)]
    [TestCase(typeof(SyntaxToolsTest), typeof(TestFixtureAttribute), true)]
    [TestCase(typeof(SyntaxToolsTest), typeof(ServiceContractAttribute), false)]
    public void GetCustomAttribute(Type type, Type attributeType, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        var actual = SyntaxTools.GetCustomAttribute(symbol, attributeType.FullName!);
        if (expected)
        {
            actual.ShouldNotBeNull();
        }
        else
        {
            actual.ShouldBeNull();
        }
    }

    [Test]
    public void ExpandInterface()
    {
        var i1Symbol = Compilation.ResolveTypeSymbol(typeof(I1));
        i1Symbol.ShouldNotBeNull();

        var actual = SyntaxTools.ExpandInterface(i1Symbol);

        actual.Count.ShouldBe(2);
        actual.ShouldContain(i1Symbol);

        var disposableSymbol = actual.First(i => !SymbolEqualityComparer.Default.Equals(i, i1Symbol));
        disposableSymbol.Name.ShouldBe(nameof(IDisposable));
    }

    [Test]
    public void ExpandClassInterface()
    {
        var s1Symbol = Compilation.ResolveTypeSymbol(typeof(S1));
        s1Symbol.ShouldNotBeNull();

        var actual = SyntaxTools.ExpandInterface(s1Symbol);

        actual.Count.ShouldBe(2);
        actual.ShouldNotContain(s1Symbol);

        actual.ShouldContain(Compilation.ResolveTypeSymbol(typeof(I1)));
    }

    [Test]
    public void GetInstanceMethods()
    {
        var i1Symbol = Compilation.ResolveTypeSymbol(typeof(I1));
        i1Symbol.ShouldNotBeNull();

        var methods = SyntaxTools.GetInstanceMethods(i1Symbol).ToList();

        methods.Count.ShouldBe(2);
    }

    [Test]
    [TestCase(typeof(I1), "ServiceModel.Grpc.DesignTime.CodeAnalysis.SyntaxToolsTest")]
    [TestCase(typeof(SyntaxToolsTest), "ServiceModel.Grpc.DesignTime.CodeAnalysis")]
    public void GetNamespace(Type type, string expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        SyntaxTools.GetNamespace(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetFullNameCases))]
    public void GetFullName(ITypeSymbol type, string expected)
    {
        SyntaxTools.GetFullName(type).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(I1), typeof(object), true)]
    [TestCase(typeof(I1), typeof(IDisposable), true)]
    [TestCase(typeof(I1), typeof(I1), true)]
    [TestCase(typeof(I1), typeof(IAliasSymbol), false)]
    public void IsAssignableFrom(Type type, Type expected, bool result)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        SyntaxTools.IsAssignableFrom(symbol, expected).ShouldBe(result);
    }

    [Test]
    public void GetClassInterfaceImplementation()
    {
        var s1Symbol = Compilation.ResolveTypeSymbol(typeof(S1));

        foreach (var method in SyntaxTools.GetInstanceMethods(Compilation.ResolveTypeSymbol(typeof(I1))))
        {
            s1Symbol.GetInterfaceImplementation(method).ShouldNotBeNull();
        }
    }

    [Test]
    public void GetInterfaceImplementation()
    {
        var i1Symbol = Compilation.ResolveTypeSymbol(typeof(I1));

        foreach (var method in SyntaxTools.GetInstanceMethods(i1Symbol))
        {
            i1Symbol.GetInterfaceImplementation(method).ShouldNotBeNull();
        }
    }

    [Test]
    [TestCase(typeof(ValueTuple<string>), true)]
    [TestCase(typeof((string, string)), true)]
    [TestCase(typeof((string, string, int)), true)]
    [TestCase(typeof(ValueTuple), false)]
    [TestCase(typeof(Tuple<string, string>), false)]
    [TestCase(typeof(object), false)]
    [TestCase(typeof((string, string)?), false)]
    public void IsValueTuple(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        SyntaxTools.IsValueTuple(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(I1), false)]
    [TestCase(typeof(object), false)]
    [TestCase(typeof(int?), true)]
    [TestCase(typeof(ValueTask?), true)]
    public void IsNullable(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        symbol.ShouldNotBeNull();

        SyntaxTools.IsNullable(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(ValueTuple<string, int>), "string", "int")]
    [TestCase(typeof(object))]
    [TestCase(typeof(Task<int>), "int")]
    public void GenericTypeArguments(Type type, params string[] expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);
        var actual = SyntaxTools.GenericTypeArguments(symbol);

        actual.Select(i => SyntaxTools.GetFullName(i)).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(Task), true)]
    [TestCase(typeof(Task<int>), true)]
    [TestCase(typeof(object), false)]
    [TestCase(typeof(Task<(int, string)>), true)]
    [TestCase(typeof(ValueTask), true)]
    [TestCase(typeof(ValueTask<int>), true)]
    public void IsTask(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);

        SyntaxTools.IsTask(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(ValueTask), true)]
    [TestCase(typeof(ValueTask<int>), true)]
    [TestCase(typeof(object), false)]
    [TestCase(typeof(Task), false)]
    public void IsValueTask(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);

        SyntaxTools.IsValueTask(symbol).ShouldBe(expected);
    }

    [Test]
    [TestCase(typeof(IAsyncEnumerable<string>), true)]
    [TestCase(typeof(IAsyncEnumerator<int>), false)]
    [TestCase(typeof(object), false)]
    public void IsAsyncEnumerable(Type type, bool expected)
    {
        var symbol = Compilation.ResolveTypeSymbol(type);

        SyntaxTools.IsAsyncEnumerable(symbol).ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetFullNameCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(FullNameCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var attribute = SyntaxTools.GetCustomAttribute(method, typeof(DisplayNameAttribute).FullName!);
            var expected = (string)attribute!.ConstructorArguments[0].Value!;
            expected.ShouldNotBeNull();

            yield return new TestCaseData(method.ReturnType, expected) { TestName = "FullName." + method.Name };
        }
    }
}