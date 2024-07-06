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

using System.IO.Pipelines;
using System.Net;
using System.Net.Mime;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

[TestFixture]
public class SwaggerUiMiddlewareTest
{
    private IList<string> _loggerOutput = null!;
    private Mock<HttpContext> _httpContext = null!;
    private Mock<HttpRequest> _httpRequest = null!;
    private Mock<HttpResponse> _httpResponse = null!;
    private Mock<IApiDescriptionAdapter> _apiAdapter = null!;
    private Mock<ISwaggerUiRequestHandler> _requestHandler = null!;
    private bool _isNextCalled;
    private ServiceModelGrpcMarker _marker = null!;
    private Mock<IMethod> _method = null!;
    private SwaggerUiMiddleware _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var logger = new MockLogger();
        _loggerOutput = logger.Output;

        var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
        loggerFactory
            .Setup(f => f.CreateLogger("ServiceModel.Grpc.SwaggerUIGateway"))
            .Returns(logger);

        _apiAdapter = new Mock<IApiDescriptionAdapter>(MockBehavior.Strict);
        _requestHandler = new Mock<ISwaggerUiRequestHandler>(MockBehavior.Strict);

        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProvider
            .Setup(p => p.GetService(typeof(IApiDescriptionAdapter)))
            .Returns(_apiAdapter.Object);
        serviceProvider
            .Setup(p => p.GetService(typeof(ISwaggerUiRequestHandler)))
            .Returns(_requestHandler.Object);

        _httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
        _httpRequest
            .SetupGet(r => r.Protocol)
            .Returns(ProtocolConstants.Http2);
        _httpRequest
            .SetupGet(r => r.ContentType)
            .Returns(ProtocolConstants.MediaTypeNameSwaggerRequest);
        _httpRequest
            .SetupGet(r => r.Path)
            .Returns("/request/path");
        _httpRequest
            .SetupGet(r => r.BodyReader)
            .Returns(new Mock<PipeReader>(MockBehavior.Strict).Object);

        _httpResponse = new Mock<HttpResponse>(MockBehavior.Strict);
        _httpResponse
            .SetupProperty(r => r.StatusCode, 0);
        _httpResponse
            .SetupGet(r => r.BodyWriter)
            .Returns(new Mock<PipeWriter>(MockBehavior.Strict).Object);

        _httpContext = new Mock<HttpContext>(MockBehavior.Strict);
        _httpContext
            .SetupGet(c => c.RequestAborted)
            .Returns(new CancellationTokenSource().Token);
        _httpContext
            .SetupGet(c => c.Request)
            .Returns(_httpRequest.Object);
        _httpContext
            .SetupGet(c => c.Response)
            .Returns(_httpResponse.Object);

        _isNextCalled = false;

        _marker = new ServiceModelGrpcMarker(new Mock<IOperationDescriptor>(MockBehavior.Strict).Object, new Mock<IMarshallerFactory>(MockBehavior.Strict).Object);

        _method = new Mock<IMethod>(MockBehavior.Strict);
        _method
            .SetupGet(m => m.Type)
            .Returns(MethodType.Unary);

        _sut = new SwaggerUiMiddleware(
            loggerFactory.Object,
            serviceProvider.Object,
            c =>
            {
                c.ShouldBe(_httpContext.Object);
                _isNextCalled.ShouldBe(false);
                _isNextCalled = true;
                return Task.CompletedTask;
            });
    }

    [Test]
    public async Task IgnoreNonSwaggerUiRequest()
    {
        _httpRequest
            .SetupGet(r => r.ContentType)
            .Returns(MediaTypeNames.Application.Json);

        await _sut.Invoke(_httpContext.Object).ConfigureAwait(false);

        _isNextCalled.ShouldBeTrue();
        _loggerOutput.ShouldBeEmpty();
    }

    [Test]
    public async Task MethodNotFound()
    {
        _apiAdapter
            .Setup(a => a.GetMarker(_httpContext.Object))
            .Returns(_marker);
        _apiAdapter
            .Setup(a => a.GetMethod(_httpContext.Object))
            .Returns((IMethod?)null);

        await _sut.Invoke(_httpContext.Object).ConfigureAwait(false);

        _isNextCalled.ShouldBeTrue();
        _apiAdapter.VerifyAll();
        _loggerOutput.Count.ShouldBe(2);
        _loggerOutput[1].ShouldStartWith("Warning: ");
    }

    [Test]
    [TestCase(MethodType.ServerStreaming)]
    [TestCase(MethodType.DuplexStreaming)]
    [TestCase(MethodType.ClientStreaming)]
    public async Task NonUnaryMethodIsNotSupported(MethodType methodType)
    {
        _method
            .SetupGet(m => m.Type)
            .Returns(methodType);

        _apiAdapter
            .Setup(a => a.GetMethod(_httpContext.Object))
            .Returns(_method.Object);

        _apiAdapter
            .Setup(a => a.GetMarker(_httpContext.Object))
            .Returns(_marker);

        await _sut.Invoke(_httpContext.Object).ConfigureAwait(false);

        _httpResponse.Object.StatusCode.ShouldBe((int)HttpStatusCode.NotImplemented);

        _isNextCalled.ShouldBeFalse();
        _apiAdapter.VerifyAll();
        _loggerOutput.Count.ShouldBe(2);
        _loggerOutput[1].ShouldStartWith("Error: ");
    }
}