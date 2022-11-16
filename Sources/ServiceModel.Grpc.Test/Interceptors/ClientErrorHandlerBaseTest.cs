// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Threading;
using Grpc.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Interceptors;

[TestFixture]
public class ClientErrorHandlerBaseTest
{
    private Mock<ClientErrorHandlerBase> _sut = null!;
    private CancellationTokenSource _tokenSource = null!;
    private ClientFaultDetail _faultDetail;
    private ClientCallInterceptorContext _callContext;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new Mock<ClientErrorHandlerBase> { CallBase = true };

        _tokenSource = new CancellationTokenSource();

        var method = new Mock<IMethod>(MockBehavior.Strict);
        _callContext = new ClientCallInterceptorContext(new CallOptions(new Metadata(), cancellationToken: _tokenSource.Token), null, method.Object);

        _faultDetail = new ClientFaultDetail(new RpcException(Status.DefaultCancelled), new object());
    }

    [Test]
    public void UserHandler()
    {
        _sut
            .Protected()
            .Setup("ThrowOrIgnoreCore", _callContext, _faultDetail)
            .Verifiable();

        _sut.Object.ThrowOrIgnore(_callContext, _faultDetail);

        _sut.Verify();
    }

    [Test]
    public void IgnoreUserHandlerOnOperationCancelled()
    {
        _tokenSource.Cancel();

        var ex = Assert.Throws<OperationCanceledException>(() => _sut.Object.ThrowOrIgnore(_callContext, _faultDetail));

        ex.ShouldNotBeNull();
        ex.CancellationToken.ShouldBe(_tokenSource.Token);
        ex.InnerException.ShouldBe(_faultDetail.OriginalError);
    }
}