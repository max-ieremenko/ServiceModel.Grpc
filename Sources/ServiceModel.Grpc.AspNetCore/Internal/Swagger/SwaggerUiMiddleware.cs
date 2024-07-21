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

using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

internal sealed class SwaggerUiMiddleware
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RequestDelegate _next;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public SwaggerUiMiddleware(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        RequestDelegate next)
    {
        _serviceProvider = serviceProvider;
        _next = next;
        _logger = loggerFactory.CreateLogger("ServiceModel.Grpc.SwaggerUIGateway");
    }

    public Task Invoke(HttpContext context)
    {
        if (IsSwaggerUiRequest(context.Request))
        {
            return InvokeSwaggerAsync(context);
        }

        return _next(context);
    }

    private static bool IsSwaggerUiRequest(HttpRequest request) =>
        ProtocolConstants.MediaTypeNameSwaggerRequest.Equals(request.ContentType, StringComparison.OrdinalIgnoreCase);

    private Task InvokeSwaggerAsync(HttpContext context)
    {
        _logger.LogInformation("Start {protocol} request {path}.", context.Request.Protocol, context.Request.Path);

        var adapter = _serviceProvider.GetRequiredService<IApiDescriptionAdapter>();

        var marker = adapter.GetMarker(context);
        if (marker == null)
        {
            _logger.LogWarning("ApiDescription for endpoint {path} not found. Ignore the request.", context.Request.Path);

            return _next(context);
        }

        var method = adapter.GetMethod(context);
        if (method == null)
        {
            _logger.LogWarning("GrpcMethodMetadata for endpoint {0} not found. Ignore the request.", context.Request.Path);

            return _next(context);
        }

        if (method.Type != MethodType.Unary)
        {
            _logger.LogError("gRPC method type {method} is not supported, endpoint {path}.", method.Type.ToString(), context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return Task.CompletedTask;
        }

        return HandleRequestAsync(context, marker.MarshallerFactory, marker.Descriptor);
    }

    private async Task HandleRequestAsync(
        HttpContext context,
        IMarshallerFactory marshallerFactory,
        IOperationDescriptor descriptor)
    {
        var handler = _serviceProvider.GetRequiredService<ISwaggerUiRequestHandler>();

        var payload = await handler
            .ReadRequestMessageAsync(context.Request.BodyReader, marshallerFactory, descriptor, context.RequestAborted)
            .ConfigureAwait(false);

        Status status;
        MemoryStream response;
        IHeaderDictionary trailers;
        using (var proxy = new GrpcProxy(context))
        {
            proxy.Attach(payload);
            await _next(context).ConfigureAwait(false);

            status = proxy.GetResponseStatus();
            trailers = proxy.Trailers;
            response = await proxy.GetResponseBody().ConfigureAwait(false);
        }

        context.Response.ContentType = ProtocolConstants.MediaTypeNameSwaggerResponse;
        if (status.StatusCode == StatusCode.OK)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            handler.AppendResponseTrailers(context.Response.Headers, trailers);

            await handler
                .WriteResponseMessageAsync(response, context.Response.BodyWriter, marshallerFactory, descriptor, context.RequestAborted)
                .ConfigureAwait(false);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await handler
                .WriteResponseErrorAsync(new RpcException(status), context.Response.BodyWriter, context.RequestAborted)
                .ConfigureAwait(false);
        }
    }
}