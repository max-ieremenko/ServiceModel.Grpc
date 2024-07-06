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
public class ResponseContextTest
{
    private MessageAccessorMock _messageAccessor = null!;
    private Mock<IStreamAccessor> _streamAccessor = null!;
    private ResponseContext _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _messageAccessor = new MessageAccessorMock();
        _streamAccessor = new Mock<IStreamAccessor>(MockBehavior.Strict);

        _sut = new ResponseContext(new MessageProxy(_messageAccessor.Accessor), _streamAccessor.Object);
    }

    [Test]
    public void CreateResponseByUserRequest()
    {
        _messageAccessor.AddProperty("p1", 0);
        _messageAccessor.SetupCreateNew();

        var stream = new object();
        _streamAccessor
            .Setup(a => a.CreateEmpty())
            .Returns(stream);

        _sut["p1"].ShouldBe(0);
        _sut.Stream.ShouldBe(stream);
    }

    [Test]
    public void CreateResponseByHandlerRequest()
    {
        _messageAccessor.AddProperty("p1", 0);
        _messageAccessor.SetupCreateNew();

        var stream = new object();
        _streamAccessor
            .Setup(a => a.CreateEmpty())
            .Returns(stream);

        var (actualResponse, actualStream) = _sut.GetRaw();

        actualResponse.ShouldBe(_messageAccessor.Message);
        actualStream.ShouldBe(stream);
    }

    [Test]
    public void ListProperties()
    {
        _messageAccessor.AddProperty("p1", 0);
        _messageAccessor.AddProperty("p2", (string?)null);
        _messageAccessor.SetupCreateNew();

        var actual = _sut.ToArray();

        actual.Length.ShouldBe(2);
        actual[0].Key.ShouldBe("p1");
        actual[0].Value.ShouldBe(0);
        actual[1].Key.ShouldBe("p2");
        actual[1].Value.ShouldBeNull();
    }

    [Test]
    public void ResponseIsProvided()
    {
        _sut.IsProvided.ShouldBeFalse();

        _sut.SetRaw(_messageAccessor.Message, null);

        _sut.IsProvided.ShouldBeTrue();
    }
}