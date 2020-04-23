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
using Shouldly;

namespace ServiceModel.Grpc.Configuration
{
    [TestFixture]
    public class DataContractMarshallerTest
    {
        [Test]
        [TestCaseSource(nameof(GetMarshallTestCases))]
        public void Marshall(object value)
        {
            var marshaller = GetType()
                .StaticMethod(nameof(MarshallTest))
                .MakeGenericMethod(value.GetType());

            marshaller.Invoke(null, new[] { value });
        }

        [Test]
        public void MarshallNull()
        {
            var content = DataContractMarshaller<string>.Default.Serializer(null);
            var actual = DataContractMarshaller<string>.Default.Deserializer(content);

            actual.ShouldBeNull();
        }

        private static void MarshallTest<T>(T value)
        {
            var content = DataContractMarshaller<T>.Default.Serializer(value);
            var actual = DataContractMarshaller<T>.Default.Deserializer(content);

            actual.ShouldBe(value);
        }

        private static IEnumerable<TestCaseData> GetMarshallTestCases()
        {
            yield return new TestCaseData("abc");
            yield return new TestCaseData(1);
            yield return new TestCaseData(1.1);
            yield return new TestCaseData(new Tuple<Tuple<string>>(new Tuple<string>("data")));
        }
    }
}
