// <copyright>
// Copyright 2021-2022 Max Ieremenko
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
using System.Linq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class RequestContextTest
{
    [Test]
    public void GetSetProperty()
    {
        var sut = new RequestContext(new MessageProxy(new[] { "p1" }, typeof(Message<int>)), null);
        sut.SetRaw(new Message<int>(), null);

        sut["p1"].ShouldBe(0);

        sut["p1"] = 10;
        sut["p1"].ShouldBe(10);
    }

    [Test]
    public void ListProperties()
    {
        var sut = new RequestContext(new MessageProxy(new[] { "p1", "p2" }, typeof(Message<int, string>)), null);
        sut.SetRaw(new Message<int, string>(10, "20"), null);

        var actual = sut.ToArray();

        actual.Length.ShouldBe(2);
        actual[0].Key.ShouldBe("p1");
        actual[0].Value.ShouldBe(10);
        actual[1].Key.ShouldBe("p2");
        actual[1].Value.ShouldBe("20");
    }

    [Test]
    public void GetSetStream()
    {
        var sut = new RequestContext(new MessageProxy(Array.Empty<string>(), typeof(Message)), new StreamProxy(typeof(int)));

        var stream1 = new[] { 1, 2 }.AsAsyncEnumerable();
        sut.SetRaw(new Message<int>(10), stream1);

        sut.Stream.ShouldBe(stream1);

        var stream2 = new[] { 1 }.AsAsyncEnumerable();
        sut.Stream = stream2;
        sut.Stream.ShouldBe(stream2);
    }

    [Test]
    public void TrySetStreamNull()
    {
        var sut = new RequestContext(new MessageProxy(Array.Empty<string>(), typeof(Message)), new StreamProxy(typeof(int)));

        var ex = Assert.Throws<ArgumentNullException>(() => sut.Stream = null);
        TestOutput.WriteLine(ex);
    }

    [Test]
    public void TrySetStreamInvalid()
    {
        var sut = new RequestContext(new MessageProxy(Array.Empty<string>(), typeof(Message)), new StreamProxy(typeof(int)));
        var stream = new[] { 1.0 }.AsAsyncEnumerable();

        var ex = Assert.Throws<InvalidCastException>(() => sut.Stream = stream);
        TestOutput.WriteLine(ex);
    }
}