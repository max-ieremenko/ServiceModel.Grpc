// <copyright>
// Copyright 2020-2023 Max Ieremenko
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
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Interceptors.Internal;

[TestFixture]
public class ServerCallErrorInterceptorTest
{
    private ServerCallErrorInterceptor _sut = null!;
    private Mock<IServerErrorHandler> _errorHandler = null!;
    private Mock<IMarshallerFactory> _marshallerFactory = null!;
    private LoggerMock _logger = null!;
    private ServerCallInterceptorContext _context;

    [SetUp]
    public void BeforeEachTest()
    {
        var serverContext = new Mock<ServerCallContext> { CallBase = true };
        _context = new ServerCallInterceptorContext(serverContext.Object);

        _marshallerFactory = new Mock<IMarshallerFactory>(MockBehavior.Strict);

        _errorHandler = new Mock<IServerErrorHandler>(MockBehavior.Strict);
        _logger = new LoggerMock();
        _sut = new ServerCallErrorInterceptor(_errorHandler.Object, _marshallerFactory.Object, _logger.Logger);
    }

    [Test]
    public void UserHandlerIgnoreError()
    {
        var error = new NotSupportedException();
        _errorHandler
            .Setup(h => h.ProvideFaultOrIgnore(_context, error))
            .Returns((ServerFaultDetail?)null);

        _sut.OnError(_context, error);

        _errorHandler.VerifyAll();
        _context.ServerCallContext.UserState.Keys.ShouldBe(new[] { ServerCallErrorInterceptor.VisitMarker });
    }

    [Test]
    public void UserHandler()
    {
        var error = new NotSupportedException();
        _errorHandler
            .Setup(h => h.ProvideFaultOrIgnore(_context, error))
            .Returns(new ServerFaultDetail
            {
                Message = "error message",
                Detail = "some detail",
                StatusCode = StatusCode.DataLoss,
                Trailers = new Metadata
                {
                    { "user-header", "user-header-value" }
                }
            });

        _marshallerFactory
            .Setup(f => f.CreateMarshaller<string>())
            .Returns(DataContractMarshallerFactory.Default.CreateMarshaller<string>());

        var ex = Assert.Throws<RpcException>(() => _sut.OnError(_context, error));

        _errorHandler.VerifyAll();
        _context.ServerCallContext.UserState.Keys.ShouldBe(new[] { ServerCallErrorInterceptor.VisitMarker });

        ex.ShouldNotBeNull();
        ex.StatusCode.ShouldBe(StatusCode.DataLoss);
        ex.Message.ShouldBe("error message");
        ex.Trailers.Count.ShouldBe(3);

        var header = ex.Trailers.First(i => "user-header".Equals(i.Key, StringComparison.OrdinalIgnoreCase));
        header.Value.ShouldBe("user-header-value");

        header = ex.Trailers.First(i => CallContext.HeaderNameErrorDetailType.Equals(i.Key, StringComparison.OrdinalIgnoreCase));
        Type.GetType(header.Value, true, false).ShouldBe(typeof(string));

        header = ex.Trailers.First(i => CallContext.HeaderNameErrorDetail.Equals(i.Key, StringComparison.OrdinalIgnoreCase));
        var detail = DataContractMarshallerFactory.Default.DeserializeHeader(typeof(string), header.ValueBytes).ShouldBeOfType<string>();
        detail.ShouldBe("some detail");
    }

    [Test]
    public void UserHandlerFaulted()
    {
        var error = new NotSupportedException();
        _errorHandler
            .Setup(h => h.ProvideFaultOrIgnore(_context, error))
            .Throws<ApplicationException>();

        Assert.Throws<ApplicationException>(() => _sut.OnError(_context, error));

        _errorHandler.VerifyAll();

        _logger.Errors.Count.ShouldBe(1);
        _logger.Errors[0].ShouldContain(nameof(ApplicationException));
    }

    [Test]
    public void CheckVisitMarker()
    {
        _context.ServerCallContext.UserState.Add(ServerCallErrorInterceptor.VisitMarker, string.Empty);

        _sut.OnError(_context, new NotSupportedException());
    }

    [Test]
    public void DetailsSerializationFaulted()
    {
        var error = new NotSupportedException();
        _errorHandler
            .Setup(h => h.ProvideFaultOrIgnore(_context, error))
            .Returns(new ServerFaultDetail
            {
                Detail = "some detail",
            });

        _marshallerFactory
            .Setup(f => f.CreateMarshaller<string>())
            .Throws<ApplicationException>();

        Assert.Throws<ApplicationException>(() => _sut.OnError(_context, error));

        _errorHandler.VerifyAll();

        _logger.Errors.Count.ShouldBe(1);
        _logger.Errors[0].ShouldContain(nameof(ApplicationException));
    }
}