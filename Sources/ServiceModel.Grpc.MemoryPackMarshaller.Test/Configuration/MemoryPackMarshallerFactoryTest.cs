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

using MemoryPack;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;

#pragma warning disable ServiceModelGrpcInternalAPI

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public partial class MemoryPackMarshallerFactoryTest
{
    private MemoryPackMarshallerFactory _sut = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _sut = new MemoryPackMarshallerFactory();

        MemoryPackMarshaller.RegisterMessageFormatter<int?>();
        MemoryPackMarshaller.RegisterMessageFormatter<int?, string>();
        MemoryPackMarshaller.RegisterMessageFormatter<int?, string, decimal?>();
        MemoryPackMarshaller.RegisterFormatter<Message<int?, string, decimal?, byte?>, MessageMemoryPackFormatter<int?, string, decimal?, byte?>>();
    }

    [Test]
    public void MessageNullTest()
    {
        RunNullTest<Message, PackableMessage>();
    }

    [Test]
    public void MessageTest()
    {
        RunTest(
            new Message(),
            new PackableMessage(),
            _ =>
            {
            },
            _ =>
            {
            });
    }

    [Test]
    public void Message1NullTest()
    {
        RunNullTest<Message<int?>, PackableMessage<int?>>();
    }

    [Test]
    [TestCase(null)]
    [TestCase(100)]
    public void Message1Test(int? value1)
    {
        RunTest(
            new Message<int?>(value1),
            new PackableMessage<int?>(value1),
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
        RunNullTest<Message<int?, string>, PackableMessage<int?, string>>();
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
            new PackableMessage<int?, string>(value1, value2),
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
        RunNullTest<Message<int?, string, decimal?>, PackableMessage<int?, string, decimal?>>();
    }

    [Test]
    [TestCase(null, null, null)]
    [TestCase(100, null, null)]
    [TestCase(null, "dummy", null)]
    [TestCase(null, null, 10)]
    [TestCase(100, "dummy", 10)]
    public void Message3Test(int? value1, string? value2, decimal? value3)
    {
        RunTest(
            new Message<int?, string, decimal?>(value1, value2, value3),
            new PackableMessage<int?, string, decimal?>(value1, value2, value3),
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
        RunNullTest<Message<int?, string, decimal?, byte?>, PackableMessage<int?, string, decimal?, byte?>>();
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
        RunTest(
            new Message<int?, string, decimal?, byte?>(value1, value2, value3, value4),
            new PackableMessage<int?, string, decimal?, byte?>(value1, value2, value3, value4),
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

    private void RunNullTest<TDomain, TPackable>()
        where TDomain : class
        where TPackable : class
    {
        var domainPayload = MarshallerSerialize((TDomain?)null);
        var actualDomain = MarshallerDeserialize<TDomain>(domainPayload);
        actualDomain.ShouldBeNull();

        var packablePayload = DefaultSerialize((TPackable?)null);
        actualDomain = MarshallerDeserialize<TDomain>(packablePayload);
        actualDomain.ShouldBeNull();

        domainPayload.ShouldBe(packablePayload);

        var actualPackable = DefaultDeserialize<TPackable>(domainPayload);
        actualPackable.ShouldBeNull();
    }

    private void RunTest<TDomain, TPackable>(TDomain domainValue, TPackable packableValue, Action<TDomain> validateDomain, Action<TPackable> validatePackable)
        where TDomain : class
        where TPackable : class
    {
        var domainPayload = MarshallerSerialize(domainValue);
        var actualDomain = MarshallerDeserialize<TDomain>(domainPayload);
        actualDomain.ShouldNotBeNull();
        validateDomain(actualDomain);

        var packablePayload = DefaultSerialize(packableValue);
        actualDomain = MarshallerDeserialize<TDomain>(packablePayload);
        actualDomain.ShouldNotBeNull();
        validateDomain(actualDomain);

        domainPayload.ShouldBe(packablePayload);

        var actualPackable = DefaultDeserialize<TPackable>(domainPayload);
        actualPackable.ShouldNotBeNull();
        validatePackable(actualPackable);
    }

    private byte[] MarshallerSerialize<T>(T value) => MarshallerExtensions.Serialize(_sut.CreateMarshaller<T>(), value);

    private T MarshallerDeserialize<T>(byte[] payload) => MarshallerExtensions.Deserialize(_sut.CreateMarshaller<T>(), payload);

    private byte[] DefaultSerialize<T>(T value) => MemoryPackSerializer.Serialize(in value);

    private T? DefaultDeserialize<T>(byte[] payload) => MemoryPackSerializer.Deserialize<T>(payload);
}