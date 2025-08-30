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

using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Domain;

#pragma warning disable ServiceModelGrpcInternalAPI

// Use an overload that does not take an ITypeShape<T> or ITypeShapeProvider
#pragma warning disable NBMsgPack051

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class NerdbankMessagePackMarshallerFactoryTest
{
    private NerdbankMessagePackMarshallerFactory _sut = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _sut = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<Anchor>();

        NerdbankMessagePackMarshaller.RegisterMessageShape<int?>();
        NerdbankMessagePackMarshaller.RegisterMessageShape<int?, string>();
        NerdbankMessagePackMarshaller.RegisterMessageShape<int?, string, decimal?>();
        TestMessage4Contract.Register<int?, string, decimal?, byte>();
    }

    [Test]
    public void MessageNullTest()
    {
        RunNullTest<Message, TestMessage>();
    }

    [Test]
    public void MessageTest()
    {
        RunTest(
            new Message(),
            new TestMessage(),
            _ => { },
            _ => { });
    }

    [Test]
    public void Message1NullTest()
    {
        RunNullTest<Message<int?>, TestMessage<int?>>();
    }

    [Test]
    [TestCase(null)]
    [TestCase(100)]
    public void Message1Test(int? value1)
    {
        RunTest(
            new Message<int?>(value1),
            new TestMessage<int?>(value1),
            actual =>
            {
                actual.Value1.ShouldBe(value1);
            },
            actual =>
            {
                actual.Value1.ShouldBe(value1);
            });
    }

    [Test]
    public void Message2NullTest()
    {
        RunNullTest<Message<int?, string>, TestMessage<int?, string>>();
    }

    [Test]
    [TestCase(null, null)]
    [TestCase(100, null)]
    [TestCase(null, "dummy")]
    [TestCase(100, "dummy")]
    public void Message2Test(int? value1, string? value2)
    {
        RunTest(
            new Message<int?, string>(value1, value2),
            new TestMessage<int?, string>(value1, value2),
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
            },
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
            });
    }

    [Test]
    public void Message3NullTest()
    {
        RunNullTest<Message<int?, string, decimal?>, TestMessage<int?, string, decimal?>>();
    }

    [Test]
    [TestCase(null, null, null)]
    [TestCase(100, null, null)]
    [TestCase(100, "dummy", null)]
    [TestCase(100, "dummy", 11)]
    [TestCase(null, "dummy", null)]
    [TestCase(null, "dummy", 11)]
    [TestCase(null, null, 11)]
    public void Message3Test(int? value1, string? value2, decimal? value3)
    {
        RunTest(
            new Message<int?, string, decimal?>(value1, value2, value3),
            new TestMessage<int?, string, decimal?>(value1, value2, value3),
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
                actual.Value3.ShouldBe(value3);
            },
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
                actual.Value3.ShouldBe(value3);
            });
    }

    [Test]
    public void Message4NullTest()
    {
        RunNullTest<TestMessage<int?, string, decimal?, byte>, TestMessage<int?, string, decimal?, byte>>();
    }

    [Test]
    [TestCase(null, null, null, 1)]
    [TestCase(100, null, null, 1)]
    [TestCase(100, "dummy", null, 1)]
    [TestCase(100, "dummy", 11, 1)]
    [TestCase(null, "dummy", null, 1)]
    [TestCase(null, "dummy", 11, 1)]
    [TestCase(null, null, 11, 1)]
    public void Message4Test(int? value1, string? value2, decimal? value3, byte value4)
    {
        RunTest(
            new TestMessage<int?, string, decimal?, byte>(value1, value2, value3, value4),
            new TestMessage<int?, string, decimal?, byte>(value1, value2, value3, value4),
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
                actual.Value3.ShouldBe(value3);
                actual.Value4.ShouldBe(value4);
            },
            actual =>
            {
                actual.Value1.ShouldBe(value1);
                actual.Value2.ShouldBe(value2);
                actual.Value3.ShouldBe(value3);
                actual.Value4.ShouldBe(value4);
            });
    }

    private void RunNullTest<TMessage, TSubstitute>()
        where TMessage : class
        where TSubstitute : class
    {
        var marshallerPayload = MarshallerSerialize((TMessage?)null);
        var actual1 = MarshallerDeserialize<TMessage>(marshallerPayload);
        actual1.ShouldBeNull();

        var defaultPayload = DefaultSerialize((TSubstitute?)null);
        var actual2 = DefaultDeserialize<TSubstitute>(defaultPayload);
        actual2.ShouldBeNull();

        actual2 = DefaultDeserialize<TSubstitute>(marshallerPayload);
        actual2.ShouldBeNull();

        actual1 = MarshallerDeserialize<TMessage>(defaultPayload);
        actual1.ShouldBeNull();

        var schema = _sut.Serializer.GetJsonSchema(_sut.GetShape<TMessage>());
        Console.WriteLine(schema);
        schema.ToString().ShouldNotContain("unknown");
    }

    private void RunTest<TMessage, TSubstitute>(TMessage value1, TSubstitute value2, Action<TMessage> validate1, Action<TSubstitute> validate2)
        where TMessage : class
        where TSubstitute : class
    {
        var marshallerPayload = MarshallerSerialize(value1);
        var actual1 = MarshallerDeserialize<TMessage>(marshallerPayload);
        actual1.ShouldNotBeNull();
        validate1(actual1);

        var defaultPayload = DefaultSerialize(value2);
        var actual2 = DefaultDeserialize<TSubstitute>(defaultPayload);
        actual2.ShouldNotBeNull();
        validate2(actual2);

        actual2 = DefaultDeserialize<TSubstitute>(marshallerPayload);
        actual2.ShouldNotBeNull();
        validate2(actual2);

        actual1 = MarshallerDeserialize<TMessage>(defaultPayload);
        actual1.ShouldNotBeNull();
        validate1(actual1);
    }

    private byte[] MarshallerSerialize<T>(T value) => MarshallerExtensions.Serialize(_sut.CreateMarshaller<T>(), value);

    private T MarshallerDeserialize<T>(byte[] payload) => MarshallerExtensions.Deserialize(_sut.CreateMarshaller<T>(), payload);

    private byte[] DefaultSerialize<T>(T value) => _sut.Serializer.Serialize(value, _sut.TypeShapeProvider);

    private T? DefaultDeserialize<T>(byte[] payload) => _sut.Serializer.Deserialize<T>(payload, _sut.TypeShapeProvider);
}