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

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class MarshallerPingPongTest
{
    private static readonly MessagePackMarshallerFactory MessagePackFactory = CreateMessagePackFactory();
    private static readonly NerdbankMessagePackMarshallerFactory NerdbankFactory = CreateNerdbankFactory();

    [Test]
    [TestCaseSource(nameof(GetMessageNullCases))]
    public void MessageNullTest(Func<Message> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessageCases))]
    public void MessageTest(Func<Message> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1NullCases))]
    public void Message1NullTest(Func<Message<int?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1ValueNullCases))]
    public void Message1ValueNullTest(Func<Message<int?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1Cases))]
    public void Message1Test(Func<Message<int?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2NullCases))]
    public void Message2NullTest(Func<Message<int?, string>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2ValueNullCases))]
    public void Message2ValueNullTest(Func<Message<int?, string>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
        actual.Value2.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2Cases))]
    public void Message2Test(Func<Message<int?, string>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3NullCases))]
    public void Message3NullTest(Func<Message<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3ValueNullCases))]
    public void Message3ValueNullTest(Func<Message<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
        actual.Value2.ShouldBeNull();
        actual.Value3.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3Cases))]
    public void Message3Test(Func<Message<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
        actual.Value3.ShouldBe(12.1);
    }

    [Test]
    [TestCaseSource(nameof(GetMessage4NullCases))]
    public void Message4NullTest(Func<TestMessage<int?, string, double?, float?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage4ValueNullCases))]
    public void Message4ValueNullTest(Func<TestMessage<int?, string, double?, float?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
        actual.Value2.ShouldBeNull();
        actual.Value3.ShouldBeNull();
        actual.Value4.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage4Cases))]
    public void Message4Test(Func<TestMessage<int?, string, double?, float?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
        actual.Value3.ShouldBe(12.1);
        actual.Value4.ShouldBe(14.2f);
    }

    private static IEnumerable<TestCaseData> GetMessageNullCases() => GetCases((Message?)null);

    private static IEnumerable<TestCaseData> GetMessageCases() => GetCases(new Message());

    private static IEnumerable<TestCaseData> GetMessage1NullCases() => GetCases((Message<int?>?)null);

    private static IEnumerable<TestCaseData> GetMessage1ValueNullCases() => GetCases(new Message<int?>());

    private static IEnumerable<TestCaseData> GetMessage1Cases() => GetCases(new Message<int?>(10));

    private static IEnumerable<TestCaseData> GetMessage2NullCases() => GetCases((Message<int?, string>?)null);

    private static IEnumerable<TestCaseData> GetMessage2ValueNullCases() => GetCases(new Message<int?, string>());

    private static IEnumerable<TestCaseData> GetMessage2Cases() => GetCases(new Message<int?, string>(10, "foo"));

    private static IEnumerable<TestCaseData> GetMessage3NullCases() => GetCases((Message<int?, string, double?>?)null);

    private static IEnumerable<TestCaseData> GetMessage3ValueNullCases() => GetCases(new Message<int?, string, double?>());

    private static IEnumerable<TestCaseData> GetMessage3Cases() => GetCases(new Message<int?, string, double?>(10, "foo", 12.1));

    private static IEnumerable<TestCaseData> GetMessage4NullCases() => GetCases((TestMessage<int?, string, double?, float?>?)null);

    private static IEnumerable<TestCaseData> GetMessage4ValueNullCases() => GetCases(new TestMessage<int?, string, double?, float?>());

    private static IEnumerable<TestCaseData> GetMessage4Cases() => GetCases(new TestMessage<int?, string, double?, float?>(10, "foo", 12.1, 14.2f));

    private static IEnumerable<TestCaseData> GetCases<T>(T? expected)
    {
        var messagePack = MessagePackFactory.CreateMarshaller<T?>();
        var nerdbank = NerdbankFactory.CreateMarshaller<T?>();
        var messagePackPayload = MarshallerExtensions.Serialize(messagePack, expected);
        var nerdbankPayload = MarshallerExtensions.Serialize(nerdbank, expected);

        yield return new TestCaseData(new Func<T?>(() => MarshallerExtensions.Deserialize(messagePack, messagePackPayload))) { TestName = "MessagePack-MessagePack" };
        yield return new TestCaseData(new Func<T?>(() => MarshallerExtensions.Deserialize(nerdbank, nerdbankPayload))) { TestName = "Nerdbank-Nerdbank" };
        yield return new TestCaseData(new Func<T?>(() => MarshallerExtensions.Deserialize(messagePack, nerdbankPayload))) { TestName = "MessagePack-Nerdbank" };
        yield return new TestCaseData(new Func<T?>(() => MarshallerExtensions.Deserialize(nerdbank, messagePackPayload))) { TestName = "Nerdbank-MessagePack" };
    }

    private static MessagePackMarshallerFactory CreateMessagePackFactory()
    {
        MessagePackMarshaller.RegisterMessageFormatter<int?>();
        MessagePackMarshaller.RegisterMessageFormatter<int?, string>();
        MessagePackMarshaller.RegisterMessageFormatter<int?, string, double?>();
        MessagePackMarshaller.RegisterFormatter<TestMessage<int?, string, double?, decimal?>, TestMessage4MessagePackFormatter<int?, string, double?, decimal?>>();

        return new MessagePackMarshallerFactory(MessagePackTools.Options);
    }

    private static NerdbankMessagePackMarshallerFactory CreateNerdbankFactory()
    {
        NerdbankMessagePackMarshaller.RegisterMessageShape<int?>();
        NerdbankMessagePackMarshaller.RegisterMessageShape<int?, string>();
        NerdbankMessagePackMarshaller.RegisterMessageShape<int?, string, double?>();
        TestMessage4Contract.Register<int?, string, double?, decimal?>();

        return new NerdbankMessagePackMarshallerFactory(NerdbankTools.Serializer, NerdbankTools.TypeShapeProvider);
    }
}