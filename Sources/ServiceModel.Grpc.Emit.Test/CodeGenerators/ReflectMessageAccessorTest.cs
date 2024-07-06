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

namespace ServiceModel.Grpc.Emit.CodeGenerators;

[TestFixture]
public class ReflectMessageAccessorTest
{
    private Message<string, int> _message = null!;
    private ReflectMessageAccessor _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _message = new Message<string, int>();
        _sut = new ReflectMessageAccessor(_message.GetType(), ["p1", "p2"]);
    }

    [Test]
    [TestCase(0, "value 1")]
    [TestCase(1, 10)]
    public void GetSetValue(int index, object expected)
    {
        _sut.SetValue(_message, index, expected);
        _sut.GetValue(_message, index).ShouldBe(expected);
    }

    [Test]
    public void CreateNew()
    {
        var actual = _sut.CreateNew();

        var message = actual.ShouldBeOfType<Message<string, int>>();
        message.Value1.ShouldBeNull();
        message.Value2.ShouldBe(0);
    }

    [Test]
    public void GetInstanceType()
    {
        _sut.GetInstanceType().ShouldBe(_message.GetType());
    }

    [Test]
    [TestCase(0, typeof(string))]
    [TestCase(1, typeof(int))]
    public void GetValueType(int index, Type expected)
    {
        _sut.GetValueType(index).ShouldBe(expected);
    }
}