// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

[TestFixture]
public partial class ApiDescriptionGeneratorTest
{
    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void GetRequestType(MethodInfo method)
    {
        var expected = method.GetCustomAttribute<RequestMetadataAttribute>();
        var message = new MessageAssembler(method);

        var parameters = ApiDescriptionGenerator.GetRequestParameters(message).ToArray();
        var headerParameters = ApiDescriptionGenerator.GetRequestHeaderParameters(message).ToArray();

        parameters.Length.ShouldBe(expected!.Parameters.Length);
        headerParameters.Length.ShouldBe(expected.HeaderParameters.Length);

        for (var i = 0; i < expected.Parameters.Length; i++)
        {
            parameters[i].ShouldBe(message.Parameters[expected.Parameters[i]]);
        }

        for (var i = 0; i < expected.HeaderParameters.Length; i++)
        {
            headerParameters[i].ShouldBe(message.Parameters[expected.HeaderParameters[i]]);
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void GetResponseType(MethodInfo method)
    {
        var expected = method.GetCustomAttribute<ResponseMetadataAttribute>();
        var message = new MessageAssembler(method);

        var responseType = ApiDescriptionGenerator.GetResponseType(message);
        var responseHeaders = ApiDescriptionGenerator.GetResponseHeaderParameters(message);

        responseType.Type.ShouldBe(expected!.Type);
        responseType.Parameter.ShouldBe(method.ReturnParameter);

        responseHeaders.Length.ShouldBe(expected.HeaderTypes.Length);
        for (var i = 0; i < expected.HeaderTypes.Length; i++)
        {
            responseHeaders[i].Type.ShouldBe(expected.HeaderTypes[i]);
            responseHeaders[i].Name.ShouldBe(expected.HeaderNames[i]);
        }
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