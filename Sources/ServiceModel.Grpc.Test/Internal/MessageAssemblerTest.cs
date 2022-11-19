// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Reflection;
using Grpc.Core;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal;

[TestFixture]
public partial class MessageAssemblerTest
{
    [Test]
    [TestCaseSource(nameof(GetResponseTypeCases))]
    public void ResponseType(
        MethodInfo method,
        Type responseType,
        Type? headerResponseType,
        int[]? headerIndexes,
        int? streamIndex)
    {
        var actual = new MessageAssembler(method);

        actual.ResponseType.ShouldBe(responseType);
        actual.HeaderResponseType.ShouldBe(headerResponseType);

        if (headerResponseType == null)
        {
            actual.HeaderResponseTypeInput.ShouldBeEmpty();
            actual.ResponseTypeIndex.ShouldBe(0);
        }
        else
        {
            actual.HeaderResponseTypeInput.ShouldBe(headerIndexes);
            actual.ResponseTypeIndex.ShouldBe(streamIndex!.Value);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetOperationTypeCases))]
    public void OperationType(MethodInfo method, MethodType expected)
    {
        new MessageAssembler(method).OperationType.ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedResponseTypeCases))]
    public void NotSupportedResponseType(MethodInfo method)
    {
        Assert.Throws<NotSupportedException>(() => new MessageAssembler(method));
    }

    [Test]
    [TestCaseSource(nameof(GetRequestTypeCases))]
    public void RequestType(
        MethodInfo method,
        Type requestType,
        int[] requestIndexes,
        Type? headerRequestType,
        int[] headerIndexes)
    {
        var sut = new MessageAssembler(method);

        sut.RequestType.ShouldBe(requestType);
        sut.RequestTypeInput.ShouldBe(requestIndexes);
        sut.HeaderRequestType.ShouldBe(headerRequestType);

        if (headerRequestType == null)
        {
            sut.HeaderRequestTypeInput.ShouldBeEmpty();
        }
        else
        {
            sut.HeaderRequestTypeInput.ShouldBe(headerIndexes);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetContextInputCases))]
    public void ContextInput(MethodInfo method, int[] expected)
    {
        var actual = new MessageAssembler(method).ContextInput;

        actual.ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedParametersCases))]
    public void NotSupportedParameters(MethodInfo method)
    {
        Assert.Throws<NotSupportedException>(() => new MessageAssembler(method));
    }

    [Test]
    [TestCaseSource(nameof(GetGenericNotSupportedCases))]
    public void GenericNotSupported(MethodInfo method)
    {
        Assert.Throws<NotSupportedException>(() => new MessageAssembler(method));
    }

    [Test]
    [TestCaseSource(nameof(GetIsCompatibleToCases))]
    public void IsCompatibleWith(MethodInfo method, MethodInfo other, bool expected)
    {
        var sut = new MessageAssembler(method);
        var otherSut = new MessageAssembler(other);

        sut.IsCompatibleWith(otherSut).ShouldBe(expected);
        otherSut.IsCompatibleWith(sut).ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetResponseTypeCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(ResponseTypeCases)))
        {
            var response = method.GetCustomAttribute<ResponseTypeAttribute>();
            var responseHeader = method.GetCustomAttribute<HeaderResponseTypeAttribute>();

            yield return new TestCaseData(
                method,
                response!.Type,
                responseHeader?.Type,
                responseHeader?.Indexes,
                responseHeader?.StreamIndex)
            {
                TestName = method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetNotSupportedResponseTypeCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(NotSupportedResponseTypeCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetOperationTypeCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(OperationTypeCases)))
        {
            var description = method.GetCustomAttribute<OperationTypeAttribute>();

            yield return new TestCaseData(
                method,
                description!.Type)
            {
                TestName = method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetRequestTypeCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(RequestTypeCases)))
        {
            var request = method.GetCustomAttribute<RequestTypeAttribute>();
            var headerRequest = method.GetCustomAttribute<HeaderRequestTypeAttribute>();

            yield return new TestCaseData(
                method,
                request!.Type,
                request!.Indexes,
                headerRequest?.Type,
                headerRequest?.Indexes)
            {
                TestName = method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetNotSupportedParametersCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(NotSupportedParametersCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetContextInputCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(ContextInputCases)))
        {
            var description = method.GetCustomAttribute<ContextInputAttribute>();

            yield return new TestCaseData(
                method,
                description!.Indexes)
            {
                TestName = method.Name
            };
        }
    }

    private static IEnumerable<TestCaseData> GetGenericNotSupportedCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(GenericNotSupportedCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetIsCompatibleToCases()
    {
        var methodByName = ReflectionTools
            .GetMethods(typeof(IsCompatibleToCases))
            .ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var method in methodByName.Values)
        {
            yield return new TestCaseData(method, method, true)
            {
                TestName = string.Format("{0} vs {1}", method.Name, method.Name)
            };

            foreach (var compatibleTo in method.GetCustomAttributes<CompatibleToAttribute>())
            {
                var other = methodByName[compatibleTo.MethodName];

                yield return new TestCaseData(method, other, true)
                {
                    TestName = string.Format("{0} vs {1}", method.Name, other.Name)
                };
            }

            foreach (var notCompatibleTo in method.GetCustomAttributes<NotCompatibleToAttribute>())
            {
                var other = methodByName[notCompatibleTo.MethodName];

                yield return new TestCaseData(method, other, false)
                {
                    TestName = string.Format("{0} vs {1}", method.Name, other.Name)
                };
            }
        }
    }
}