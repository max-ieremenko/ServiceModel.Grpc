// <copyright>
// Copyright 2020 Max Ieremenko
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
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class DataContractMarshallerTest
{
    [Test]
    [TestCaseSource(nameof(GetMarshallTestCases))]
    public void Marshall(object value)
    {
        var payload = MarshallerExtensions.SerializeObject(DataContractMarshallerFactory.Default, value);

        var actual = MarshallerExtensions.DeserializeObject(DataContractMarshallerFactory.Default, value.GetType(), payload);

        actual.ShouldBe(value);
    }

    [Test]
    public void MarshallNull()
    {
        var payload = MarshallerExtensions.Serialize(DataContractMarshaller<string>.Default, null!);

        var actual = MarshallerExtensions.Deserialize(DataContractMarshaller<string>.Default, payload);

        actual.ShouldBeNull();
    }

    private static IEnumerable<TestCaseData> GetMarshallTestCases()
    {
        yield return new TestCaseData("abc");
        yield return new TestCaseData(1);
        yield return new TestCaseData(1.1);
        yield return new TestCaseData(new Tuple<Tuple<string>>(new Tuple<string>("data")));
    }
}