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
