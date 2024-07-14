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
using ServiceModel.Grpc.Api;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class RequestContextTest
{
    private MessageAccessorMock _messageAccessor = null!;
    private Mock<IStreamAccessor> _streamAccessor = null!;
    private RequestContext _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _messageAccessor = new MessageAccessorMock();
        _streamAccessor = new Mock<IStreamAccessor>(MockBehavior.Strict);

        _sut = new RequestContext(_messageAccessor.Accessor, _streamAccessor.Object);
    }

    [Test]
    public void GetSetProperty()
    {
        _messageAccessor.AddProperty("p1", 0);
        _sut.SetRaw(_messageAccessor.Message, null);

        _sut["p1"].ShouldBe(0);

        _sut["p1"] = 10;
        _sut["p1"].ShouldBe(10);
    }

    [Test]
    public void ListProperties()
    {
        _messageAccessor.AddProperty("p1", 10);
        _messageAccessor.AddProperty("p2", "20");

        _sut.SetRaw(_messageAccessor.Message, null);

        var actual = _sut.ToArray();

        actual.Length.ShouldBe(2);
        actual[0].Key.ShouldBe("p1");
        actual[0].Value.ShouldBe(10);
        actual[1].Key.ShouldBe("p2");
        actual[1].Value.ShouldBe("20");
    }

    [Test]
    public void GetSetStream()
    {
        var stream1 = new[] { 1, 2 };
        _sut.SetRaw(_messageAccessor.Message, stream1);

        _sut.Stream.ShouldBe(stream1);

        var stream2 = new[] { 1 };
        _streamAccessor
            .Setup(a => a.Validate(stream2));

        _sut.Stream = stream2;
        _sut.Stream.ShouldBe(stream2);

        _streamAccessor.VerifyAll();
    }

    [Test]
    public void TrySetStreamNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Stream = null);
    }

    [Test]
    public void TrySetStreamInvalid()
    {
        var stream = new[] { 1.0 };

        _streamAccessor
            .Setup(a => a.Validate(stream))
            .Throws<InvalidCastException>();

        Assert.Throws<InvalidCastException>(() => _sut.Stream = stream);
    }
}