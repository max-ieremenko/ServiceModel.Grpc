// <copyright>
// Copyright 2021 Max Ieremenko
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
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class MessageProxyTest
{
    private Message<string, int> _message = null!;
    private MessageProxy _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _message = new Message<string, int>();
        _sut = new MessageProxy(new[] { "p1", "p2" }, _message.GetType());
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
    public void CreateDefault()
    {
        var actual = _sut.CreateDefault();

        var message = actual.ShouldBeOfType<Message<string, int>>();
        message.Value1.ShouldBeNull();
        message.Value2.ShouldBe(0);
    }
}