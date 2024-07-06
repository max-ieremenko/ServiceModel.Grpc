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

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using ServiceModel.Grpc.Emit;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

[TestFixture]
public partial class ApiDescriptionGeneratorTest
{
    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void GetRequestType(MethodInfo method)
    {
        var metadata = method.GetCustomAttribute<RequestMetadataAttribute>().ShouldNotBeNull();
        var descriptor = EmitGenerator.GenerateOperationDescriptor(() => method);

        var actual = ApiDescriptionGenerator.GetRequestParameters(descriptor);

        actual.Length.ShouldBe(metadata.Parameters.Length + metadata.HeaderParameters.Length);

        for (var i = 0; i < metadata.HeaderParameters.Length; i++)
        {
            var expected = method.GetParameters()[metadata.HeaderParameters[i]];
            actual[i].Parameter.ShouldBe(expected);
            actual[i].Source.ShouldBe(BindingSource.Header);
        }

        for (var i = 0; i < metadata.Parameters.Length; i++)
        {
            var expected = method.GetParameters()[metadata.Parameters[i]];
            actual[i + metadata.HeaderParameters.Length].Parameter.ShouldBe(expected);
            actual[i + metadata.HeaderParameters.Length].Source.ShouldBe(BindingSource.Form);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void GetResponseType(MethodInfo method)
    {
        var expected = method.GetCustomAttribute<ResponseMetadataAttribute>().ShouldNotBeNull();
        var descriptor = EmitGenerator.GenerateOperationDescriptor(() => method);

        var responseType = ApiDescriptionGenerator.GetResponseType(descriptor);
        var responseHeaders = ApiDescriptionGenerator.GetResponseHeaderParameters(descriptor);

        responseType.Type.ShouldBe(expected.Type);
        responseType.Parameter.ShouldBe(method.ReturnParameter);

        responseHeaders.Length.ShouldBe(expected.HeaderTypes.Length);
        for (var i = 0; i < expected.HeaderTypes.Length; i++)
        {
            responseHeaders[i].Type.ShouldBe(expected.HeaderTypes[i]);
            responseHeaders[i].Name.ShouldBe(expected.HeaderNames[i]);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void MethodSignature(MethodInfo method)
    {
        var expected = method.GetCustomAttribute<SignatureAttribute>().ShouldNotBeNull();
        var descriptor = EmitGenerator.GenerateOperationDescriptor(() => method);

        var actual = MethodSignatureBuilder.Build(
            method.Name,
            ApiDescriptionGenerator.GetRequestParameters(descriptor),
            ApiDescriptionGenerator.GetResponseType(descriptor).Type,
            ApiDescriptionGenerator.GetResponseHeaderParameters(descriptor));

        actual.ShouldBe(expected.Signature);
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        foreach (var method in ReflectionTools.GetMethods(typeof(TestCases)))
        {
            yield return new TestCaseData(method)
            {
                TestName = method.Name
            };
        }
    }
}