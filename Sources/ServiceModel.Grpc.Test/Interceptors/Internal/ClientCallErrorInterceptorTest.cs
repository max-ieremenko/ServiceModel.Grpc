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

using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Interceptors.Internal
{
    [TestFixture]
    public class ClientCallErrorInterceptorTest
    {
        private ClientCallErrorInterceptor _sut;
        private Mock<IClientErrorHandler> _errorHandler;
        private LoggerMock _logger;
        private ClientCallInterceptorContext _context;
        private RpcException _error;

        [SetUp]
        public void BeforeEachTest()
        {
            _logger = new LoggerMock();
            _errorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict);
            _context = new ClientCallInterceptorContext(default, "host", new Mock<IMethod>(MockBehavior.Strict).Object);
            _error = new RpcException(Status.DefaultSuccess, new Metadata());

            _sut = new ClientCallErrorInterceptor(_errorHandler.Object, DataContractMarshallerFactory.Default, _logger.Logger);
        }

        [Test]
        public void UserHandlerWithDetail()
        {
            _error.Trailers.Add(
                CallContext.HeaderNameErrorDetailType,
                typeof(string).GetShortAssemblyQualifiedName());
            _error.Trailers.Add(
                CallContext.HeaderNameErrorDetail,
                _sut.MarshallerFactory.SerializeHeader("abc"));

            _errorHandler
                .Setup(h => h.ThrowOrIgnore(_context, It.IsAny<ClientFaultDetail>()))
                .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
                {
                    detail.OriginalError.ShouldBe(_error);
                    detail.Detail.ShouldBe("abc");
                });

            _sut.OnError(_context, _error);

            _errorHandler.VerifyAll();
            _logger.Errors.ShouldBeEmpty();
        }

        [Test]
        public void UserHandlerNoDetails()
        {
            _errorHandler
                .Setup(h => h.ThrowOrIgnore(_context, It.IsAny<ClientFaultDetail>()))
                .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
                {
                    detail.OriginalError.ShouldBe(_error);
                    detail.Detail.ShouldBeNull();
                });

            _sut.OnError(_context, _error);

            _errorHandler.VerifyAll();
            _logger.Errors.ShouldBeEmpty();
        }

        [Test]
        public void FailToResolveDetailType()
        {
            _error.Trailers.Add(
                CallContext.HeaderNameErrorDetailType,
                "invalid type");

            _errorHandler
                .Setup(h => h.ThrowOrIgnore(_context, It.IsAny<ClientFaultDetail>()))
                .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
                {
                    detail.OriginalError.ShouldBe(_error);
                    detail.Detail.ShouldBeNull();
                });

            _sut.OnError(_context, _error);

            _errorHandler.VerifyAll();
            _logger.Errors.Count.ShouldBe(1);
            _logger.Errors[0].ShouldContain("invalid type");
        }
    }
}
