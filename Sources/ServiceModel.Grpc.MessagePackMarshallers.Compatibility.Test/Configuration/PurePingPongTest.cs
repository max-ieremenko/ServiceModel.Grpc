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
using ServiceModel.Grpc.Domain;

namespace ServiceModel.Grpc.Configuration;

[TestFixture]
public class PurePingPongTest
{
    [Test]
    [TestCaseSource(nameof(GetMessageNullCases))]
    public void MessageNullTest(Func<TestMessage> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessageCases))]
    public void MessageTest(Func<TestMessage> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1NullCases))]
    public void Message1NullTest(Func<TestMessage<int?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1ValueNullCases))]
    public void Message1ValueNullTest(Func<TestMessage<int?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage1Cases))]
    public void Message1Test(Func<TestMessage<int?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2NullCases))]
    public void Message2NullTest(Func<TestMessage<int?, string>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2ValueNullCases))]
    public void Message2ValueNullTest(Func<TestMessage<int?, string>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
        actual.Value2.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage2Cases))]
    public void Message2Test(Func<TestMessage<int?, string>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3NullCases))]
    public void Message3NullTest(Func<TestMessage<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3ValueNullCases))]
    public void Message3ValueNullTest(Func<TestMessage<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBeNull();
        actual.Value2.ShouldBeNull();
        actual.Value3.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage3Cases))]
    public void Message3Test(Func<TestMessage<int?, string, double?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
        actual.Value3.ShouldBe(12.1);
    }

    [Test]
    [TestCaseSource(nameof(GetMessage4NullCases))]
    public void Message4NullTest(Func<TestMessage<int?, string, double?, decimal?>> source)
    {
        var actual = source();
        actual.ShouldBeNull();
    }

    [Test]
    [TestCaseSource(nameof(GetMessage4ValueNullCases))]
    public void Message4ValueNullTest(Func<TestMessage<int?, string, double?, decimal?>> source)
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
    public void Message4Test(Func<TestMessage<int?, string, double?, decimal?>> source)
    {
        var actual = source();
        actual.ShouldNotBeNull();
        actual.Value1.ShouldBe(10);
        actual.Value2.ShouldBe("foo");
        actual.Value3.ShouldBe(12.1);
        actual.Value4.ShouldBe(14.2m);
    }

    private static IEnumerable<TestCaseData> GetMessageNullCases() => GetCases((TestMessage?)null);

    private static IEnumerable<TestCaseData> GetMessageCases() => GetCases(new TestMessage());

    private static IEnumerable<TestCaseData> GetMessage1NullCases() => GetCases((TestMessage<int?>?)null);

    private static IEnumerable<TestCaseData> GetMessage1ValueNullCases() => GetCases(new TestMessage<int?>());

    private static IEnumerable<TestCaseData> GetMessage1Cases() => GetCases(new TestMessage<int?>(10));

    private static IEnumerable<TestCaseData> GetMessage2NullCases() => GetCases((TestMessage<int?, string>?)null);

    private static IEnumerable<TestCaseData> GetMessage2ValueNullCases() => GetCases(new TestMessage<int?, string>());

    private static IEnumerable<TestCaseData> GetMessage2Cases() => GetCases(new TestMessage<int?, string>(10, "foo"));

    private static IEnumerable<TestCaseData> GetMessage3NullCases() => GetCases((TestMessage<int?, string, double?>?)null);

    private static IEnumerable<TestCaseData> GetMessage3ValueNullCases() => GetCases(new TestMessage<int?, string, double?>());

    private static IEnumerable<TestCaseData> GetMessage3Cases() => GetCases(new TestMessage<int?, string, double?>(10, "foo", 12.1));

    private static IEnumerable<TestCaseData> GetMessage4NullCases() => GetCases((TestMessage<int?, string, double?, decimal?>?)null);

    private static IEnumerable<TestCaseData> GetMessage4ValueNullCases() => GetCases(new TestMessage<int?, string, double?, decimal?>());

    private static IEnumerable<TestCaseData> GetMessage4Cases() => GetCases(new TestMessage<int?, string, double?, decimal?>(10, "foo", 12.1, 14.2m));

    private static IEnumerable<TestCaseData> GetCases<T>(T? expected)
    {
        var messagePackPayload = MessagePackTools.Serialize(expected);
        var nerdbankPayload = NerdbankTools.Serialize(expected);

        yield return new TestCaseData(new Func<T?>(() => MessagePackTools.Deserialize<T>(messagePackPayload))) { TestName = "MessagePack-MessagePack" };
        yield return new TestCaseData(new Func<T?>(() => NerdbankTools.Deserialize<T>(nerdbankPayload))) { TestName = "Nerdbank-Nerdbank" };
        yield return new TestCaseData(new Func<T?>(() => MessagePackTools.Deserialize<T>(nerdbankPayload))) { TestName = "MessagePack-Nerdbank" };
        yield return new TestCaseData(new Func<T?>(() => NerdbankTools.Deserialize<T>(messagePackPayload))) { TestName = "Nerdbank-MessagePack" };
    }
}