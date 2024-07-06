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
using Moq.Protected;
using NUnit.Framework;

namespace ServiceModel.Grpc.Interceptors;

[TestFixture]
public class ServerErrorHandlerBaseTest
{
    private Mock<ServerErrorHandlerBase> _sut = null!;
    private Mock<ServerCallContext> _rpcContext = null!;
    private CancellationTokenSource _tokenSource = null!;
    private ServerCallInterceptorContext _errorContext;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new Mock<ServerErrorHandlerBase> { CallBase = true };

        _tokenSource = new CancellationTokenSource();

        _rpcContext = new Mock<ServerCallContext>(MockBehavior.Strict);
        _rpcContext
            .Protected()
            .SetupGet<CancellationToken>("CancellationTokenCore")
            .Returns(_tokenSource.Token);

        _errorContext = new ServerCallInterceptorContext(_rpcContext.Object);
    }

    [Test]
    public void UserHandler()
    {
        var ex = new NotSupportedException();

        _sut
            .Protected()
            .Setup("ProvideFaultOrIgnoreCore", _errorContext, ex)
            .Verifiable();

        _sut.Object.ProvideFaultOrIgnore(_errorContext, ex).ShouldBeNull();

        _sut.Verify();
    }

    [Test]
    public void IgnoreUserHandlerOnOperationCancelled()
    {
        var ex = new NotSupportedException();

        _sut
            .Protected()
            .Setup("OnOperationCancelled", _errorContext, ex)
            .Verifiable();

        _tokenSource.Cancel();
        _sut.Object.ProvideFaultOrIgnore(_errorContext, ex).ShouldBeNull();

        _sut.Verify();
    }
}