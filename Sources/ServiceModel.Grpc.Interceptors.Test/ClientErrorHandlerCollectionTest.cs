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

using Grpc.Core;
using NUnit.Framework;

namespace ServiceModel.Grpc.Interceptors;

[TestFixture]
public class ClientErrorHandlerCollectionTest
{
    private ClientErrorHandlerCollection _sut = null!;
    private CancellationTokenSource _tokenSource = null!;
    private ClientFaultDetail _faultDetail;
    private ClientCallInterceptorContext _callContext;
    private Mock<IClientErrorHandler> _errorHandler1 = null!;
    private Mock<IClientErrorHandler> _errorHandler2 = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ClientErrorHandlerCollection();

        _tokenSource = new CancellationTokenSource();

        var method = new Mock<IMethod>(MockBehavior.Strict);
        _callContext = new ClientCallInterceptorContext(new CallOptions(new Metadata(), cancellationToken: _tokenSource.Token), null, method.Object);

        _faultDetail = new ClientFaultDetail(new RpcException(Status.DefaultCancelled), new object());

        _errorHandler1 = new Mock<IClientErrorHandler>(MockBehavior.Strict);
        _errorHandler2 = new Mock<IClientErrorHandler>(MockBehavior.Strict);
    }

    [Test]
    public void DefaultCtor()
    {
        _sut.Pipeline.ShouldBeEmpty();
    }

    [Test]
    public void Ctor()
    {
        var sut = new ClientErrorHandlerCollection(_errorHandler1.Object);

        sut.Pipeline.ShouldBe(new[] { _errorHandler1.Object });
    }

    [Test]
    public void Add()
    {
        _sut.Add(_errorHandler1.Object);

        _sut.Pipeline.ShouldBe(new[] { _errorHandler1.Object });
    }

    [Test]
    public void ThrowOrIgnoreFirst()
    {
        _sut.Add(_errorHandler1.Object);
        _sut.Add(_errorHandler2.Object);

        var error = new ApplicationException();

        _errorHandler1
            .Setup(h => h.ThrowOrIgnore(_callContext, _faultDetail))
            .Throws(error);

        var actual = Assert.Throws<ApplicationException>(() => _sut.ThrowOrIgnore(_callContext, _faultDetail));

        actual.ShouldBe(error);
    }

    [Test]
    public void ThrowOrIgnoreLast()
    {
        _sut.Add(_errorHandler1.Object);
        _sut.Add(_errorHandler2.Object);

        var error = new ApplicationException();

        _errorHandler1
            .Setup(h => h.ThrowOrIgnore(_callContext, _faultDetail));
        _errorHandler2
            .Setup(h => h.ThrowOrIgnore(_callContext, _faultDetail))
            .Throws(error);

        var actual = Assert.Throws<ApplicationException>(() => _sut.ThrowOrIgnore(_callContext, _faultDetail));

        actual.ShouldBe(error);
        _errorHandler1.VerifyAll();
    }
}