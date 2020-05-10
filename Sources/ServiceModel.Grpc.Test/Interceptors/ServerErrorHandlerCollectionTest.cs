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

namespace ServiceModel.Grpc.Interceptors
{
    [TestFixture]
    public class ServerErrorHandlerCollectionTest
    {
        private ServerErrorHandlerCollection _sut;
        private Mock<IServerErrorHandler> _errorHandler1;
        private Mock<IServerErrorHandler> _errorHandler2;
        private Mock<ServerCallContext> _rpcContext;
        private CancellationTokenSource _tokenSource;
        private ServerCallInterceptorContext _errorContext;

        [SetUp]
        public void BeforeEachTest()
        {
            _errorHandler1 = new Mock<IServerErrorHandler>(MockBehavior.Strict);
            _errorHandler2 = new Mock<IServerErrorHandler>(MockBehavior.Strict);

            _sut = new ServerErrorHandlerCollection();

            _tokenSource = new CancellationTokenSource();

            _rpcContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            _rpcContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            _errorContext = new ServerCallInterceptorContext(_rpcContext.Object);
        }

        [Test]
        public void DefaultCtor()
        {
            _sut.Pipeline.ShouldBeEmpty();
        }

        [Test]
        public void Ctor()
        {
            var sut = new ServerErrorHandlerCollection(_errorHandler1.Object);

            sut.Pipeline.ShouldBe(new[] { _errorHandler1.Object });
        }

        [Test]
        public void Add()
        {
            _sut.Add(_errorHandler1.Object);

            _sut.Pipeline.ShouldBe(new[] { _errorHandler1.Object });
        }

        [Test]
        public void ProvideFaultOrIgnoreFromFirst()
        {
            _sut.Add(_errorHandler1.Object);
            _sut.Add(_errorHandler2.Object);

            var error = new Exception();
            var expected = new ServerFaultDetail
            {
                Detail = new object()
            };

            _errorHandler1
                .Setup(h => h.ProvideFaultOrIgnore(_errorContext, error))
                .Returns(expected);

            var actual = _sut.ProvideFaultOrIgnore(_errorContext, error);

            actual.ShouldNotBeNull();
            actual.Value.ShouldBe(expected);
        }

        [Test]
        public void ProvideFaultOrIgnoreFromLast()
        {
            _sut.Add(_errorHandler1.Object);
            _sut.Add(_errorHandler2.Object);

            var error = new Exception();
            var expected = new ServerFaultDetail
            {
                Detail = new object()
            };

            _errorHandler1
                .Setup(h => h.ProvideFaultOrIgnore(_errorContext, error))
                .Returns((ServerFaultDetail?)null);

            _errorHandler2
                .Setup(h => h.ProvideFaultOrIgnore(_errorContext, error))
                .Returns(expected);

            var actual = _sut.ProvideFaultOrIgnore(_errorContext, error);

            actual.ShouldNotBeNull();
            actual.Value.ShouldBe(expected);
            _errorHandler1.VerifyAll();
        }
    }
}
