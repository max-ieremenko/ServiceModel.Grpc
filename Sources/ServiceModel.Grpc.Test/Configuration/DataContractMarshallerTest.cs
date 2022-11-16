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
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.Internal.IO;
using Shouldly;

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class DataContractMarshallerTest
{
    [Test]
    [TestCaseSource(nameof(GetMarshallTestCases))]
    public void Marshall(object value)
    {
        var marshaller = GetType()
            .InstanceMethod(nameof(MarshallTest))
            .MakeGenericMethod(value.GetType());

        marshaller.Invoke(this, new[] { value });
    }

    [Test]
    public void MarshallNull()
    {
        byte[] content;
        using (var serializationContext = new DefaultSerializationContext())
        {
            DataContractMarshaller<string>.Default.ContextualSerializer(null!, serializationContext);
            content = serializationContext.GetContent();
        }

        var actual = DataContractMarshaller<string>.Default.ContextualDeserializer(new DefaultDeserializationContext(content));

        actual.ShouldBeNull();
    }

    private static IEnumerable<TestCaseData> GetMarshallTestCases()
    {
        yield return new TestCaseData("abc");
        yield return new TestCaseData(1);
        yield return new TestCaseData(1.1);
        yield return new TestCaseData(new Tuple<Tuple<string>>(new Tuple<string>("data")));
    }

    private void MarshallTest<T>(T value)
    {
        byte[] content;
        using (var serializationContext = new DefaultSerializationContext(10 * 1024))
        {
            DataContractMarshaller<T>.Default.ContextualSerializer(value, serializationContext);
            content = serializationContext.GetContent();
        }

        var actual = DataContractMarshaller<T>.Default.ContextualDeserializer(new DefaultDeserializationContext(content));

        actual.ShouldBe(value);
    }
}