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

using System.Collections.Generic;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors.Api;
using Shouldly;

namespace ServiceModel.Grpc.Interceptors.Internal;

[TestFixture]
public class ClientCallErrorInterceptorTest
{
    private ClientCallErrorInterceptor _sut = null!;
    private Mock<IClientErrorHandler> _errorHandler = null!;
    private List<string> _loggerErrors = null!;
    private MarshallerFactoryMock _marshallerFactory = null!;
    private ClientCallInterceptorContext _context;
    private RpcException _error = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _loggerErrors = new List<string>();

        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger
            .Setup(l => l.LogError(It.IsNotNull<string>(), It.IsNotNull<object[]>()))
            .Callback<string, object[]>((message, args) => _loggerErrors.Add(string.Format(message, args)));

        _marshallerFactory = new MarshallerFactoryMock();
        _errorHandler = new Mock<IClientErrorHandler>(MockBehavior.Strict);
        _context = new ClientCallInterceptorContext(default, "host", new Mock<IMethod>(MockBehavior.Strict).Object);
        _error = new RpcException(Status.DefaultSuccess, new Metadata());

        _sut = new ClientCallErrorInterceptor(_errorHandler.Object, _marshallerFactory.Factory, logger.Object);
    }

    [Test]
    public void UserHandlerWithDetail()
    {
        _marshallerFactory.SetupString();

        _error.Trailers.Add(
            Headers.HeaderNameErrorDetailType,
            typeof(string).GetShortAssemblyQualifiedName());
        _error.Trailers.Add(
            Headers.HeaderNameErrorDetail,
            MarshallerExtensions.SerializeObject(_sut.MarshallerFactory, "abc"));

        _errorHandler
            .Setup(h => h.ThrowOrIgnore(_context, It.IsAny<ClientFaultDetail>()))
            .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
            {
                detail.OriginalError.ShouldBe(_error);
                detail.Detail.ShouldBe("abc");
            });

        _sut.OnError(_context, _error);

        _errorHandler.VerifyAll();
        _loggerErrors.ShouldBeEmpty();
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
        _loggerErrors.ShouldBeEmpty();
    }

    [Test]
    public void FailToResolveDetailType()
    {
        _marshallerFactory.SetupString();

        _error.Trailers.Add(
            Headers.HeaderNameErrorDetailType,
            "invalid type");
        _error.Trailers.Add(
            Headers.HeaderNameErrorDetail,
            MarshallerExtensions.SerializeObject(_sut.MarshallerFactory, "dummy"));

        _errorHandler
            .Setup(h => h.ThrowOrIgnore(_context, It.IsAny<ClientFaultDetail>()))
            .Callback<ClientCallInterceptorContext, ClientFaultDetail>((_, detail) =>
            {
                detail.OriginalError.ShouldBe(_error);
                detail.Detail.ShouldBeNull();
            });

        _sut.OnError(_context, _error);

        _errorHandler.VerifyAll();
        _loggerErrors.Count.ShouldBe(1);
        _loggerErrors[0].ShouldContain("invalid type");
    }
}