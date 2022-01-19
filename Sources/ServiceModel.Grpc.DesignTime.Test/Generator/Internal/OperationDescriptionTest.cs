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
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    [TestFixture]
    public partial class OperationDescriptionTest
    {
        private static readonly Compilation Compilation = CSharpCompilation
            .Create(
                nameof(OperationDescriptionTest),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(OperationDescriptionTest).Assembly.Location)
                });

        [Test]
        [TestCaseSource(nameof(GetResponseTypeCases))]
        public void ResponseType(
            IMethodSymbol method,
            string className,
            string? valueTypeName,
            string? headerClassName,
            int[]? headerIndexes,
            string[]? headerValueTypeName,
            int? streamIndex)
        {
            var actual = new OperationDescription(method, "s1", ServiceContract.GetServiceOperationName(method));

            actual.ResponseType.ClassName.ShouldBe(className);
            if (valueTypeName == null)
            {
                actual.ResponseType.Properties.Length.ShouldBe(0);
            }
            else
            {
                actual.ResponseType.Properties.Length.ShouldBe(1);
                actual.ResponseType.Properties[0].ShouldBe(valueTypeName);
            }

            if (headerClassName == null)
            {
                actual.HeaderResponseType.ShouldBeNull();
                actual.HeaderResponseTypeInput.ShouldBeEmpty();
                actual.ResponseTypeIndex.ShouldBe(0);
            }
            else
            {
                actual.HeaderResponseType.ShouldNotBeNull();
                actual.HeaderResponseType.ClassName.ShouldBe(headerClassName);
                actual.HeaderResponseType.Properties.ShouldBe(headerValueTypeName);
                actual.HeaderResponseTypeInput.ShouldBe(headerIndexes);
                actual.ResponseTypeIndex.ShouldBe(streamIndex!.Value);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetOperationTypeCases))]
        public void OperationType(IMethodSymbol method, MethodType expectedType)
        {
            var actual = new OperationDescription(method, "s1", ServiceContract.GetServiceOperationName(method));

            actual.OperationType.ShouldBe(expectedType);
        }

        [Test]
        [TestCaseSource(nameof(GetNotSupportedResponseTypeCases))]
        public void NotSupportedResponseType(IMethodSymbol method)
        {
            var ex = Assert.Throws<NotSupportedException>(() => new OperationDescription(method, "s1", "dummy"));

            ex.ShouldNotBeNull();
            ex.Message.ShouldContain(method.Name);
        }

        [Test]
        [TestCaseSource(nameof(GetRequestTypeCases))]
        public void RequestType(
            IMethodSymbol method,
            string requestClassName,
            int[] requestIndexes,
            string[] requestValueTypeName,
            string? headerClassName,
            int[] headerIndexes,
            string[]? headerValueTypeName)
        {
            var actual = new OperationDescription(method, "s1", ServiceContract.GetServiceOperationName(method));

            actual.RequestType.ClassName.ShouldBe(requestClassName);
            actual.RequestType.Properties.ShouldBe(requestValueTypeName);
            actual.RequestTypeInput.ShouldBe(requestIndexes);

            if (headerClassName == null)
            {
                actual.HeaderRequestType.ShouldBeNull();
                actual.HeaderRequestTypeInput.Length.ShouldBe(0);
            }
            else
            {
                actual.HeaderRequestType.ShouldNotBeNull();
                actual.HeaderRequestType.ClassName.ShouldBe(headerClassName);
                actual.HeaderRequestTypeInput.ShouldBe(headerIndexes);
                actual.HeaderRequestType.Properties.ShouldBe(headerValueTypeName);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetContextInputCases))]
        public void ContextInput(IMethodSymbol method, int[] indexes)
        {
            var actual = new OperationDescription(method, "s1", ServiceContract.GetServiceOperationName(method));

            actual.ContextInput.ShouldBe(indexes);
        }

        [Test]
        [TestCaseSource(nameof(GetNotSupportedParametersCases))]
        public void NotSupportedParameters(IMethodSymbol method)
        {
            var ex = Assert.Throws<NotSupportedException>(() => new OperationDescription(method, "s1", "dummy"));

            ex.ShouldNotBeNull();
            ex.Message.ShouldContain(method.Name);
        }

        [Test]
        [TestCaseSource(nameof(GetGenericNotSupportedCases))]
        public void GenericNotSupported(IMethodSymbol method)
        {
            var ex = Assert.Throws<NotSupportedException>(() => new OperationDescription(method, "s1", "dummy"));

            ex.ShouldNotBeNull();
            ex.Message.ShouldContain(method.Name);
        }

        [Test]
        [TestCaseSource(nameof(GetIsCompatibleToCases))]
        public void IsCompatibleWith(IMethodSymbol method, IMethodSymbol other, bool expected)
        {
            var sut = new OperationDescription(method, "service", "operation");
            var otherSut = new OperationDescription(other, "service", "operation");

            sut.IsCompatibleWith(otherSut).ShouldBe(expected);
            otherSut.IsCompatibleWith(sut).ShouldBe(expected);
        }

        private static IEnumerable<TestCaseData> GetResponseTypeCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(ResponseTypeCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                var response = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(ResponseTypeAttribute));
                var responseHeader = method.GetAttributes().FirstOrDefault(i => i.AttributeClass!.Name == nameof(HeaderResponseTypeAttribute));

                yield return new TestCaseData(
                    method,
                    response.ConstructorArguments[0].Value,
                    response.ConstructorArguments[1].Value,
                    responseHeader?.ConstructorArguments[0].Value,
                    responseHeader?.ConstructorArguments[1].Values.Select(i => (int)i.Value!).ToArray(),
                    responseHeader?.ConstructorArguments[2].Values.Select(i => (string)i.Value!).ToArray(),
                    responseHeader?.ConstructorArguments[3].Value)
                {
                    TestName = "ResponseType." + method.Name
                };
            }
        }

        private static IEnumerable<TestCaseData> GetRequestTypeCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(RequestTypeCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                var request = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(RequestTypeAttribute));
                var headerRequest = method.GetAttributes().FirstOrDefault(i => i.AttributeClass!.Name == nameof(HeaderRequestTypeAttribute));

                yield return new TestCaseData(
                    method,
                    request.ConstructorArguments[0].Value,
                    request.ConstructorArguments[1].Values.Select(i => (int)i.Value!).ToArray(),
                    request.ConstructorArguments[2].Values.Select(i => (string)i.Value!).ToArray(),
                    headerRequest?.ConstructorArguments[0].Value,
                    headerRequest?.ConstructorArguments[1].Values.Select(i => (int)i.Value!).ToArray(),
                    headerRequest?.ConstructorArguments[2].Values.Select(i => (string)i.Value!).ToArray())
                {
                    TestName = "RequestType." + method.Name
                };
            }
        }

        private static IEnumerable<TestCaseData> GetContextInputCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(ContextInputCases));
            type.ShouldNotBeNull();

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
            var type = Compilation.GetTypeByMetadataName(typeof(NotSupportedResponseTypeCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                yield return new TestCaseData(method) { TestName = "ResponseType." + method.Name };
            }
        }

        private static IEnumerable<TestCaseData> GetNotSupportedParametersCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(NotSupportedParametersCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                yield return new TestCaseData(method) { TestName = "Parameters." + method.Name };
            }
        }

        private static IEnumerable<TestCaseData> GetOperationTypeCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(OperationTypeCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                var description = method.GetAttributes().First(i => i.AttributeClass!.Name == nameof(OperationTypeAttribute));
                var methodType = (string)description.ConstructorArguments[0].Value!;

                yield return new TestCaseData(
                    method,
                    Enum.Parse<MethodType>(methodType))
                {
                    TestName = "OperationType." + method.Name
                };
            }
        }

        private static IEnumerable<TestCaseData> GetGenericNotSupportedCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(GenericNotSupportedCases));
            type.ShouldNotBeNull();

            foreach (var method in SyntaxTools.GetInstanceMethods(type))
            {
                yield return new TestCaseData(method) { TestName = method.Name };
            }
        }

        private static IEnumerable<TestCaseData> GetIsCompatibleToCases()
        {
            var type = Compilation.GetTypeByMetadataName(typeof(IsCompatibleToCases));
            type.ShouldNotBeNull();

#pragma warning disable RS1024 // Compare symbols correctly
            var methodByName = SyntaxTools
                .GetInstanceMethods(type)
                .ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
#pragma warning restore RS1024 // Compare symbols correctly

            foreach (var method in methodByName.Values)
            {
                yield return new TestCaseData(method, method, true)
                {
                    TestName = string.Format("{0} vs {1}", method.Name, method.Name)
                };

                foreach (var compatibleTo in method.GetAttributes().Where(i => i.AttributeClass!.Name == nameof(CompatibleToAttribute)))
                {
                    var otherName = (string)compatibleTo.ConstructorArguments[0].Value!;
                    var other = methodByName[otherName];

                    yield return new TestCaseData(method, other, true)
                    {
                        TestName = string.Format("{0} vs {1}", method.Name, other.Name)
                    };
                }

                foreach (var notCompatibleTo in method.GetAttributes().Where(i => i.AttributeClass!.Name == nameof(NotCompatibleToAttribute)))
                {
                    var otherName = (string)notCompatibleTo.ConstructorArguments[0].Value!;
                    var other = methodByName[otherName];

                    yield return new TestCaseData(method, other, false)
                    {
                        TestName = string.Format("{0} vs {1}", method.Name, other.Name)
                    };
                }
            }
        }
    }
}
