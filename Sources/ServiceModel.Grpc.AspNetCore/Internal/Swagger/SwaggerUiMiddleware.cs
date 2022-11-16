// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    private static bool IsSwaggerUiRequest(HttpRequest request)
    {
        return ProtocolConstants.MediaTypeNameSwaggerRequest.Equals(request.ContentType, StringComparison.OrdinalIgnoreCase);
    }

    private static IList<string> OrderedRequestParameterNames(ApiDescription description)
    {
        var result = new List<string>(description.ParameterDescriptions.Count);
        for (var i = 0; i < description.ParameterDescriptions.Count; i++)
        {
            var parameter = description.ParameterDescriptions[i];
            if (parameter.Source == BindingSource.Form)
            {
                result.Add(parameter.Name);
            }
        }

        return result;
    }

    private Task InvokeSwaggerAsync(HttpContext context)
    {
        _logger.LogInformation(
            "Start {0} request {1}.",
            context.Request.Protocol,
            context.Request.Path);

        var adapter = _serviceProvider.GetRequiredService<IApiDescriptionAdapter>();

        var description = adapter.FindApiDescription(context.Request.Path);
        if (description == null)
        {
            _logger.LogWarning(
                "ApiDescription for endpoint {0} not found. Ignore the request.",
                context.Request.Path);

            return _next(context);
        }

        var method = adapter.GetMethod(context);
        if (method == null)
        {
            _logger.LogWarning(
                "GrpcMethodMetadata for endpoint {0} not found. Ignore the request.",
                context.Request.Path);

            return _next(context);
        }

        if (method.Type != MethodType.Unary)
        {
            _logger.LogError(
                "gRPC method type {0} is not supported, endpoint {1}.",
                method.Type.ToString(),
                context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return Task.CompletedTask;
        }

        return HandleRequestAsync(context, OrderedRequestParameterNames(description), method);
    }

    private async Task HandleRequestAsync(HttpContext context, IList<string> orderedRequestParameterNames, IMethod method)
    {
        var handler = _serviceProvider.GetRequiredService<ISwaggerUiRequestHandler>();

        var payload = await handler
            .ReadRequestMessageAsync(context.Request.BodyReader, orderedRequestParameterNames, method, context.RequestAborted)
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
                .WriteResponseMessageAsync(response, context.Response.BodyWriter, method, context.RequestAborted)
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