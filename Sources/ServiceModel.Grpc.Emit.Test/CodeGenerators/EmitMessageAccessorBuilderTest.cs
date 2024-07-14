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

using System.Linq.Expressions;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public class EmitMessageAccessorBuilderTest
{
    private Type _sutType = null!;
    private Func<string[], IMessageAccessor> _factory = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _sutType = EmitMessageAccessorBuilder.GetMessageAccessorGenericType(typeof(Message<,>));

        var type = _sutType.MakeGenericType(typeof(int), typeof(string));

        var names = Expression.Parameter(typeof(string[]), "names");

        var factory = Expression.New(
            type.Constructor(typeof(string[])),
            names);

        _factory = Expression.Lambda<Func<string[], IMessageAccessor>>(factory, names).Compile();
    }

    [Test]
    public void Ctor_NullNames()
    {
        TestOutput.WriteLine(_sutType.Constructor(1).Disassemble());

        Should.Throw<ArgumentNullException>(() => _factory(null!));
    }

    [Test]
    public void Ctor_NamesLength()
    {
        TestOutput.WriteLine(_sutType.Constructor(1).Disassemble());

        Should.Throw<ArgumentOutOfRangeException>(() => _factory(["p1"]));
    }

    [Test]
    public void Names()
    {
        TestOutput.WriteLine(_sutType.InstanceProperty(nameof(IMessageAccessor.Names)).GetMethod!.Disassemble());

        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        sut.Names.ShouldBe(["p1", "p2"]);
    }

    [Test]
    public void CreateNew()
    {
        TestOutput.WriteLine(_sutType.InstanceMethod(nameof(IMessageAccessor.CreateNew)).Disassemble());

        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        var actual = sut.CreateNew().ShouldBeOfType<Message<int, string>>().ShouldNotBeNull();

        actual.Value1.ShouldBe(0);
        actual.Value1 = 10;
        actual.Value1.ShouldBe(10);

        actual.Value2.ShouldBeNull();
        actual.Value2 = "dummy";
        actual.Value2.ShouldBe("dummy");
    }

    [Test]
    public void GetInstanceType()
    {
        TestOutput.WriteLine(_sutType.InstanceMethod(nameof(IMessageAccessor.GetInstanceType)).Disassemble());

        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        sut.GetInstanceType().ShouldBe(typeof(Message<int, string>));
    }

    [Test]
    public void GetValueType()
    {
        TestOutput.WriteLine(_sutType.InstanceMethod(nameof(IMessageAccessor.GetValueType)).Disassemble());

        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        sut.GetValueType(0).ShouldBe(typeof(int));
        sut.GetValueType(1).ShouldBe(typeof(string));

        Should.Throw<ArgumentOutOfRangeException>(() => sut.GetValueType(2));
    }

    [Test]
    public void GetValue()
    {
        TestOutput.WriteLine(_sutType.InstanceMethod(nameof(IMessageAccessor.GetValue)).Disassemble());

        var message = new Message<int, string>();
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();

        sut.GetValue(message, 0).ShouldBe(0);
        sut.GetValue(message, 1).ShouldBeNull();
        Should.Throw<ArgumentOutOfRangeException>(() => sut.GetValue(message, 2));

        message.Value1 = 10;
        sut.GetValue(message, 0).ShouldBe(10);

        message.Value2 = "dummy";
        sut.GetValue(message, 1).ShouldBe("dummy");
    }

    [Test]
    public void GetValue_NullMessage()
    {
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();

        Should.Throw<ArgumentNullException>(() => sut.GetValue(null!, 0));
    }

    [Test]
    public void GetValue_InvalidCast()
    {
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        var message = new Message<string, string>();

        Should.Throw<InvalidCastException>(() => sut.GetValue(message, 0));
    }

    [Test]
    public void SetValue()
    {
        TestOutput.WriteLine(_sutType.InstanceMethod(nameof(IMessageAccessor.SetValue)).Disassemble());

        var message = new Message<int, string>();
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();

        sut.SetValue(message, 0, 10);
        message.Value1.ShouldBe(10);

        sut.SetValue(message, 1, "dummy");
        message.Value2.ShouldBe("dummy");

        sut.SetValue(message, 1, null);
        message.Value2.ShouldBeNull();

        Should.Throw<ArgumentOutOfRangeException>(() => sut.SetValue(message, 2, null));
    }

    [Test]
    public void SetValue_NullMessage()
    {
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();

        Should.Throw<ArgumentNullException>(() => sut.SetValue(null!, 0, null));
    }

    [Test]
    public void SetValue_MessageInvalidCast()
    {
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        var message = new Message<string, string>();

        Should.Throw<InvalidCastException>(() => sut.SetValue(message, 1, null));
    }

    [Test]
    public void SetValue_ValueInvalidCast()
    {
        var sut = _factory(["p1", "p2"]).ShouldNotBeNull();
        var message = new Message<int, string>();

        Should.Throw<InvalidCastException>(() => sut.SetValue(message, 0, "dummy"));
    }
}