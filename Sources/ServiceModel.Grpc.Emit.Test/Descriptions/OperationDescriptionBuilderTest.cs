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
using Grpc.Core;
using NUnit.Framework;

namespace ServiceModel.Grpc.Emit.Descriptions;

[TestFixture]
public partial class OperationDescriptionBuilderTest
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
        var actual = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");

        actual.ShouldNotBeNull();
        actual.ResponseType.Properties.ShouldBe(responseType.GetGenericArguments());

        if (headerResponseType == null)
        {
            actual.HeaderResponseType.ShouldBeNull();
            actual.HeaderResponseTypeInput.ShouldBeEmpty();
            actual.ResponseTypeIndex.ShouldBe(0);
        }
        else
        {
            actual.HeaderResponseType.ShouldNotBeNull();
            actual.HeaderResponseType.Properties.ShouldBe(headerResponseType.GenericTypeArguments);
            actual.HeaderResponseTypeInput.ShouldBe(headerIndexes);
            actual.ResponseTypeIndex.ShouldBe(streamIndex!.Value);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetOperationTypeCases))]
    public void OperationType(MethodInfo method, MethodType expected)
    {
        var actual = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");

        actual.ShouldNotBeNull();
        actual.OperationType.ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedResponseTypeCases))]
    public void NotSupportedResponseType(MethodInfo method)
    {
        Should.Throw<NotSupportedException>(() => ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy"));
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
        var actual = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");

        actual.ShouldNotBeNull();
        actual.RequestType.Properties.ShouldBe(requestType.GetGenericArguments());
        actual.RequestTypeInput.ShouldBe(requestIndexes);

        if (headerRequestType == null)
        {
            actual.HeaderRequestType.ShouldBeNull();
            actual.HeaderRequestTypeInput.ShouldBeEmpty();
        }
        else
        {
            actual.HeaderRequestType.ShouldNotBeNull();
            actual.HeaderRequestType.Properties.ShouldBe(headerRequestType.GetGenericArguments());
            actual.HeaderRequestTypeInput.ShouldBe(headerIndexes);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetContextInputCases))]
    public void ContextInput(MethodInfo method, int[] expected)
    {
        var actual = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");

        actual.ShouldNotBeNull();
        actual.ContextInput.ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetNotSupportedParametersCases))]
    public void NotSupportedParameters(MethodInfo method)
    {
        Should.Throw<NotSupportedException>(() => ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy"));
    }

    [Test]
    [TestCaseSource(nameof(GetGenericNotSupportedCases))]
    public void GenericNotSupported(MethodInfo method)
    {
        Should.Throw<NotSupportedException>(() => ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy"));
    }

    [Test]
    [TestCaseSource(nameof(GetResponseHeaderNamesCases))]
    public void GetResponseHeaderNames(MethodInfo method, string[] expected)
    {
        var operation = ContractDescriptionBuilder.BuildOperation(method, "dummy", "dummy");

        var actual = operation.ShouldNotBeNull().GetResponseHeaderNames();
        actual.ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetResponseTypeCases()
    {
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(ResponseTypeCases)))
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
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(NotSupportedResponseTypeCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetOperationTypeCases()
    {
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(OperationTypeCases)))
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
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(RequestTypeCases)))
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
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(NotSupportedParametersCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetContextInputCases()
    {
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(ContextInputCases)))
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
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(GenericNotSupportedCases)))
        {
            yield return new TestCaseData(method) { TestName = method.Name };
        }
    }

    private static IEnumerable<TestCaseData> GetResponseHeaderNamesCases()
    {
        foreach (var method in ReflectionTools.GetInstanceMethods(typeof(ResponseHeaderNamesCases)))
        {
            var expected = method.GetCustomAttribute<ResponseHeaderNamesAttribute>();

            yield return new TestCaseData(method, expected!.Names)
            {
                TestName = method.Name
            };
        }
    }
}