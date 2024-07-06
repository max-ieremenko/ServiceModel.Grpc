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

using System;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Internal;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class MessageProxyTest
{
    private Mock<IMessageAccessor> _messageAccessor = null!;
    private MessageProxy _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _messageAccessor = new Mock<IMessageAccessor>(MockBehavior.Strict);
        _messageAccessor
            .SetupGet(a => a.Names)
            .Returns(["p1", "p2"]);

        _sut = new MessageProxy(_messageAccessor.Object);
    }

    [Test]
    [TestCase("p1", 0)]
    [TestCase("P1", 0)]
    [TestCase("p2", 1)]
    [TestCase("P2", 1)]
    [TestCase("unknown", -1)]
    public void GetPropertyIndex(string name, int expected)
    {
        if (expected < 0)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetPropertyIndex(name));
        }
        else
        {
            _sut.GetPropertyIndex(name).ShouldBe(expected);
        }

        _messageAccessor.VerifyAll();
    }

    [Test]
    [TestCase(0, "value 1")]
    [TestCase(1, 10)]
    public void SetValue(int index, object expected)
    {
        var message = new object();
        _messageAccessor
            .Setup(a => a.SetValue(message, index, expected));

        _sut.SetValue(message, index, expected);

        _messageAccessor.VerifyAll();
    }

    [Test]
    [TestCase(0, "value 1")]
    [TestCase(1, 10)]
    public void GetValue(int index, object expected)
    {
        var message = new object();
        _messageAccessor
            .Setup(a => a.GetValue(message, index))
            .Returns(expected);

        _sut.GetValue(message, index).ShouldBe(expected);

        _messageAccessor.VerifyAll();
    }

    [Test]
    public void CreateDefault()
    {
        var expected = new object();
        _messageAccessor
            .Setup(a => a.CreateNew())
            .Returns(expected);

        var actual = _sut.CreateDefault();

        actual.ShouldBe(expected);
    }
}