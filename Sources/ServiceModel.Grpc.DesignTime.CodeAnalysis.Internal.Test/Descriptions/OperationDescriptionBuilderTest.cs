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
using System.Linq;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceModel.Grpc.DesignTime.CodeAnalysis.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.CodeAnalysis.Descriptions;

[TestFixture]
public partial class OperationDescriptionBuilderTest
{
    private static readonly CSharpCompilation Compilation = CSharpCompilationExtensions.CreateDefault();

    [Test]
    [TestCaseSource(nameof(GetResponseTypeCases))]
    public void ResponseType(
        IMethodSymbol method,
        INamedTypeSymbol? valueType,
        int[]? headerIndexes,
        INamedTypeSymbol[]? headerValueType,
        int? streamIndex)
    {
        var actual = Build(method, "s1", ServiceContract.GetServiceOperationName(method));

        if (valueType == null)
        {
            actual.ResponseType.Properties.Length.ShouldBe(0);
        }
        else
        {
            actual.ResponseType.Properties.Length.ShouldBe(1);
            var actualValueType = actual.ResponseType.Properties[0].ShouldBeAssignableTo<INamedTypeSymbol>().ShouldNotBeNull();
            if (valueType.IsTupleType)
            {
                actualValueType.IsTupleType.ShouldBeTrue();
                actualValueType.TupleUnderlyingType.ShouldBe(valueType, SymbolEqualityComparer.Default);
            }
            else
            {
                actual.ResponseType.Properties[0].ShouldBe(valueType, SymbolEqualityComparer.Default);
            }
        }

        if (headerValueType == null)
        {
            actual.HeaderResponseType.ShouldBeNull();
            actual.HeaderResponseTypeInput.ShouldBeEmpty();
            actual.ResponseTypeIndex.ShouldBe(0);
        }
        else
        {
            actual.HeaderResponseType.ShouldNotBeNull();
            actual.HeaderResponseType.Properties.ShouldBe(headerValueType, SymbolEqualityComparer.Default);
            actual.HeaderResponseTypeInput.ShouldBe(headerIndexes);
            actual.ResponseTypeIndex.ShouldBe(streamIndex!.Value);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetOperationTypeCases))]
    public void OperationType(IMethodSymbol method, MethodType expectedType)
    {
        var actual = Build(method, "s1", ServiceContract.GetServiceOperationName(method));

        actual.OperationType.ShouldBe(expectedType);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedResponseTypeCases))]
    public void NotSupportedResponseType(IMethodSymbol method)
    {
        BuildFail(method);
    }

    [Test]
    [TestCaseSource(nameof(GetRequestTypeCases))]
    public void RequestType(
        IMethodSymbol method,
        int[] requestIndexes,
        INamedTypeSymbol[] requestValueType,
        int[] headerIndexes,
        INamedTypeSymbol[]? headerValueType)
    {
        var actual = Build(method, "s1", ServiceContract.GetServiceOperationName(method));

        actual.RequestType.Properties.ShouldBe(requestValueType, SymbolEqualityComparer.Default);
        actual.RequestTypeInput.ShouldBe(requestIndexes);

        if (headerValueType == null)
        {
            actual.HeaderRequestType.ShouldBeNull();
            actual.HeaderRequestTypeInput.Length.ShouldBe(0);
        }
        else
        {
            actual.HeaderRequestType.ShouldNotBeNull();
            actual.HeaderRequestTypeInput.ShouldBe(headerIndexes);
            actual.HeaderRequestType.Properties.ShouldBe(headerValueType, SymbolEqualityComparer.Default);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetContextInputCases))]
    public void ContextInput(IMethodSymbol method, int[] indexes)
    {
        var actual = Build(method, "s1", ServiceContract.GetServiceOperationName(method));

        actual.ContextInput.ShouldBe(indexes);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedParametersCases))]
    public void NotSupportedParameters(IMethodSymbol method)
    {
        BuildFail(method);
    }

    [Test]
    [TestCaseSource(nameof(GetGenericNotSupportedCases))]
    public void GenericNotSupported(IMethodSymbol method)
    {
        BuildFail(method);
    }

    [Test]
    [TestCaseSource(nameof(GetIsCompatibleToCases))]
    public void IsCompatibleWith(IMethodSymbol method, IMethodSymbol other, bool expected)
    {
        var sut = Build(method, "service", "operation");
        var otherSut = Build(other, "service", "operation");

        sut.IsCompatibleWith(otherSut).ShouldBe(expected);
        otherSut.IsCompatibleWith(sut).ShouldBe(expected);
    }

    private static OperationDescription Build(IMethodSymbol method, string serviceName, string operationName)
    {
        var sut = new OperationDescriptionBuilder(method, serviceName, operationName);
        sut.TryBuild(out var actual, out var error).ShouldBeTrue(error);
        return actual.ShouldNotBeNull();
    }

    private static void BuildFail(IMethodSymbol method)
    {
        new OperationDescriptionBuilder(method, "s1", "dummy")
            .TryBuild(out _, out var actual)
            .ShouldBeFalse();

        actual.ShouldNotBeNull().ShouldContain(method.Name);
    }

    private static IEnumerable<TestCaseData> GetResponseTypeCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(ResponseTypeCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var response = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(ResponseTypeAttribute));
            var responseHeader = method.GetAttributes().FirstOrDefault(i => i.AttributeClass!.Name == nameof(HeaderResponseTypeAttribute));

            yield return new TestCaseData(
                method,
                response.ConstructorArguments[0].Value,
                responseHeader?.ConstructorArguments[0].Values.Select(i => (int)i.Value!).ToArray(),
                responseHeader?.ConstructorArguments[1].Values.Select(i => (INamedTypeSymbol)i.Value!).ToArray(),
                responseHeader?.ConstructorArguments[2].Value)
            {
                TestName = "ResponseType." + method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetRequestTypeCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(RequestTypeCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var request = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(RequestTypeAttribute));
            var headerRequest = method.GetAttributes().FirstOrDefault(i => i.AttributeClass!.Name == nameof(HeaderRequestTypeAttribute));

            yield return new TestCaseData(
                method,
                request.ConstructorArguments[0].Values.Select(i => (int)i.Value!).ToArray(),
                request.ConstructorArguments[1].Values.Select(i => (INamedTypeSymbol)i.Value!).ToArray(),
                headerRequest?.ConstructorArguments[0].Values.Select(i => (int)i.Value!).ToArray(),
                headerRequest?.ConstructorArguments[1].Values.Select(i => (INamedTypeSymbol)i.Value!).ToArray())
            {
                TestName = "RequestType." + method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetContextInputCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(ContextInputCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var description = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(ContextInputAttribute));

            yield return new TestCaseData(
                method,
                description.ConstructorArguments[0].Values.Select(i => (int)i.Value!).ToArray())
            {
                TestName = "ContextInput." + method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetNotSupportedResponseTypeCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(NotSupportedResponseTypeCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            yield return new TestCaseData(method) { TestName = "ResponseType." + method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetNotSupportedParametersCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(NotSupportedParametersCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            yield return new TestCaseData(method) { TestName = "Parameters." + method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetOperationTypeCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(OperationTypeCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            var description = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(OperationTypeAttribute));
            var methodType = (string)description.ConstructorArguments[0].Value!;

            yield return new TestCaseData(
                method,
                Enum.Parse(typeof(MethodType), methodType))
            {
                TestName = "OperationType." + method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetGenericNotSupportedCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(GenericNotSupportedCases));

        foreach (var method in SyntaxTools.GetInstanceMethods(type))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetIsCompatibleToCases()
    {
        var type = Compilation.ResolveTypeSymbol(typeof(IsCompatibleToCases));

#pragma warning disable RS1024 // Compare symbols correctly
        var methodByName = SyntaxTools
            .GetInstanceMethods(type)
            .ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
#pragma warning restore RS1024 // Compare symbols correctly

        foreach (var method in methodByName.Values)
        {
            yield return new TestCaseData(method, method, true)
            {
                TestName = $"{method.Name} vs {method.Name}"
            };

            foreach (var compatibleTo in method.GetAttributes().Where(i => i.AttributeClass!.Name == nameof(CompatibleToAttribute)))
            {
                var otherName = (string)compatibleTo.ConstructorArguments[0].Value!;
                var other = methodByName[otherName];

                yield return new TestCaseData(method, other, true)
                {
                    TestName = $"{method.Name} vs {other.Name}"
                };
            }

            foreach (var notCompatibleTo in method.GetAttributes().Where(i => i.AttributeClass!.Name == nameof(NotCompatibleToAttribute)))
            {
                var otherName = (string)notCompatibleTo.ConstructorArguments[0].Value!;
                var other = methodByName[otherName];

                yield return new TestCaseData(method, other, false)
                {
                    TestName = $"{method.Name} vs {other.Name}"
                };
            }
        }
    }
}