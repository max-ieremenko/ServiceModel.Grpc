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

using MessagePack;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Configuration.Formatters;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace ServiceModel.Grpc.MessagePackMarshaller.Configuration;

[TestFixture]
public partial class MessagePackMarshallerFactoryTest
{
    private MessagePackSerializerOptions _defaultOptions = null!;
    private MessagePackMarshallerFactory _sut = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _defaultOptions = MessagePackSerializer.DefaultOptions;
        _sut = new MessagePackMarshallerFactory(_defaultOptions);

        Grpc.Configuration.MessagePackMarshaller.RegisterMessageFormatter<int?>();
        Grpc.Configuration.MessagePackMarshaller.RegisterMessageFormatter<int?, string>();
        Grpc.Configuration.MessagePackMarshaller.RegisterMessageFormatter<int?, string, decimal?>();
        Grpc.Configuration.MessagePackMarshaller.RegisterFormatter<Message<int?, string, decimal?, byte?>, MessageMessagePackFormatter<int?, string, decimal?, byte?>>();
    }

    [Test]
    public void MessageNullTest()
    {
#if DEBUG
        var formatter = MessageFormatterResolver.Instance.GetFormatter<Message>().ShouldBeOfType<MessageMessagePackFormatter>();
        formatter.DeserializeCallsCounter = 0;
        formatter.SerializeCallsCounter = 0;
#endif

        RunNullTest<Message>();

#if DEBUG
        formatter.DeserializeCallsCounter.ShouldBe(2);
        formatter.SerializeCallsCounter.ShouldBe(1);
#endif
    }

    [Test]
    public void MessageTest()
    {
        RunTest(new Message(), _ =>
        {
        });
    }

    [Test]
    public void Message1NullTest()
    {
#if DEBUG
        var formatter = MessageFormatterResolver.Instance.GetFormatter<Message<int?>>().ShouldBeOfType<MessageMessagePackFormatter<int?>>();
        formatter.DeserializeCallsCounter = 0;
        formatter.SerializeCallsCounter = 0;
#endif

        RunNullTest<Message<int?>>();

#if DEBUG
        formatter.DeserializeCallsCounter.ShouldBe(2);
        formatter.SerializeCallsCounter.ShouldBe(1);
#endif
    }

    [Test]
    [TestCase(null)]
    [TestCase(100)]
    public void Message1Test(int? value1)
    {
        RunTest(new Message<int?>(value1), actual =>
        {
            actual.Value1.ShouldBe(value1);
        });
    }

    [Test]
    public void Message2NullTest()
    {
#if DEBUG
        var formatter = MessageFormatterResolver.Instance.GetFormatter<Message<int?, string>>().ShouldBeOfType<MessageMessagePackFormatter<int?, string>>();
        formatter.DeserializeCallsCounter = 0;
        formatter.SerializeCallsCounter = 0;
#endif

        RunNullTest<Message<int?, string>>();

#if DEBUG
        formatter.DeserializeCallsCounter.ShouldBe(2);
        formatter.SerializeCallsCounter.ShouldBe(1);
#endif
    }

    [Test]
    [TestCase(null, null)]
    [TestCase(100, null)]
    [TestCase(null, "dummy")]
    [TestCase(100, "dummy")]
    public void Message2Test(int? value1, string? value2)
    {
        RunTest(new Message<int?, string>(value1, value2), actual =>
        {
            actual.Value1.ShouldBe(value1);
            actual.Value2.ShouldBe(value2);
        });
    }

    [Test]
    public void Message3NullTest()
    {
#if DEBUG
        var formatter = MessageFormatterResolver.Instance.GetFormatter<Message<int?, string, decimal?>>().ShouldBeOfType<MessageMessagePackFormatter<int?, string, decimal?>>();
        formatter.DeserializeCallsCounter = 0;
        formatter.SerializeCallsCounter = 0;
#endif

        RunNullTest<Message<int?, string, decimal?>>();

#if DEBUG
        formatter.DeserializeCallsCounter.ShouldBe(2);
        formatter.SerializeCallsCounter.ShouldBe(1);
#endif
    }

    [Test]
    [TestCase(null, null, null)]
    [TestCase(100, null, null)]
    [TestCase(null, "dummy", null)]
    [TestCase(null, null, 10)]
    [TestCase(100, "dummy", 10)]
    public void Message3Test(int? value1, string? value2, decimal? value3)
    {
        RunTest(new Message<int?, string, decimal?>(value1, value2, value3), actual =>
        {
            actual.Value1.ShouldBe(value1);
            actual.Value2.ShouldBe(value2);
            actual.Value3.ShouldBe(value3);
        });
    }

    [Test]
    public void Message4NullTest()
    {
        var formatter = MessageFormatterResolver.Instance.GetFormatter<Message<int?, string, decimal?, byte?>>().ShouldBeOfType<MessageMessagePackFormatter<int?, string, decimal?, byte?>>();
        formatter.DeserializeCallsCounter = 0;
        formatter.SerializeCallsCounter = 0;

        RunNullTest<Message<int?, string, decimal?, byte?>>();

        formatter.DeserializeCallsCounter.ShouldBe(2);
        formatter.SerializeCallsCounter.ShouldBe(1);
    }

    [Test]
    [TestCase(null, null, null, null)]
    [TestCase(100, null, null, null)]
    [TestCase(null, "dummy", null, null)]
    [TestCase(null, null, 10, null)]
    [TestCase(null, null, null, 200)]
    [TestCase(100, "dummy", 10, 200)]
    public void Message4Test(int? value1, string? value2, decimal? value3, byte? value4)
    {
        RunTest(new Message<int?, string, decimal?, byte?>(value1, value2, value3, value4), actual =>
        {
            actual.Value1.ShouldBe(value1);
            actual.Value2.ShouldBe(value2);
            actual.Value3.ShouldBe(value3);
            actual.Value4.ShouldBe(value4);
        });
    }

    private void RunNullTest<T>()
        where T : class
    {
        var defaultPayload = DefaultSerialize((T?)null);
        var actual = DefaultDeserialize<T>(defaultPayload);
        actual.ShouldBeNull();

        var marshallerPayload = MarshallerSerialize((T?)null);
        actual = MarshallerDeserialize<T>(marshallerPayload);
        actual.ShouldBeNull();

        defaultPayload.ShouldBe(marshallerPayload);

        actual = MarshallerDeserialize<T>(defaultPayload);
        actual.ShouldBeNull();

        actual = DefaultDeserialize<T>(defaultPayload);
        actual.ShouldBeNull();
    }

    private void RunTest<T>(T value, Action<T> validate)
        where T : class
    {
        var defaultPayload = DefaultSerialize(value);
        var actual = DefaultDeserialize<T>(defaultPayload);
        actual.ShouldNotBeNull();
        validate(actual);

        var marshallerPayload = MarshallerSerialize(value);
        actual = MarshallerDeserialize<T>(marshallerPayload);
        actual.ShouldNotBeNull();
        validate(actual);

        defaultPayload.ShouldBe(marshallerPayload);

        actual = MarshallerDeserialize<T>(defaultPayload);
        actual.ShouldNotBeNull();
        validate(actual);

        actual = DefaultDeserialize<T>(defaultPayload);
        actual.ShouldNotBeNull();
        validate(actual);
    }

    private byte[] MarshallerSerialize<T>(T value) => MarshallerExtensions.Serialize(_sut.CreateMarshaller<T>(), value);

    private T MarshallerDeserialize<T>(byte[] payload) => MarshallerExtensions.Deserialize(_sut.CreateMarshaller<T>(), payload);

    private byte[] DefaultSerialize<T>(T value)
    {
        using (var buffer = new MemoryStream())
        {
            MessagePackSerializer.Serialize(buffer, value, _defaultOptions);
            return buffer.ToArray();
        }
    }

    private T DefaultDeserialize<T>(byte[] payload)
    {
        using (var buffer = new MemoryStream(payload))
        {
            return MessagePackSerializer.Deserialize<T>(buffer, _defaultOptions);
        }
    }
}