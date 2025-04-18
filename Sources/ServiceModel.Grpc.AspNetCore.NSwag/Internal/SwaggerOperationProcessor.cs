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

using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using ServiceModel.Grpc.AspNetCore.Internal;
using ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

namespace ServiceModel.Grpc.AspNetCore.NSwag.Internal;

internal sealed class SwaggerOperationProcessor : IOperationProcessor
{
    private const string Multipart = "multipart/form-data";

    private readonly ServiceModelGrpcSwaggerOptions _configuration;

    public SwaggerOperationProcessor(IOptions<ServiceModelGrpcSwaggerOptions> configuration)
    {
        _configuration = configuration.Value;
    }

    public bool Process(OperationProcessorContext context)
    {
        var description = (context as AspNetCoreOperationProcessorContext)?.ApiDescription;
        var descriptor = description?.ActionDescriptor as GrpcActionDescriptor;
        if (descriptor == null)
        {
            return true;
        }

        var operation = context.OperationDescription.Operation;
        FixRequestType(operation);

        if (_configuration.AutogenerateOperationSummaryAndDescription)
        {
            UpdateSummary(operation, descriptor);
            UpdateDescription(operation, descriptor);
        }

        for (var i = 0; i < description!.SupportedResponseTypes.Count; i++)
        {
            AddResponseHeaders(operation, description.SupportedResponseTypes[i], context.SchemaResolver, context.SchemaGenerator);
        }

        return true;
    }

    private static void UpdateSummary(OpenApiOperation operation, GrpcActionDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(operation.Summary))
        {
            operation.Summary = string.Format(CultureInfo.InvariantCulture, "ServiceModel.Grpc - {0}", descriptor.MethodType.ToString());
        }
        else
        {
            operation.Summary = string.Format(CultureInfo.InvariantCulture, "ServiceModel.Grpc - {0}. {1}", descriptor.MethodType.ToString(), operation.Summary);
        }
    }

    private static void UpdateDescription(OpenApiOperation operation, GrpcActionDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(operation.Description))
        {
            operation.Description = WebUtility.HtmlEncode(descriptor.MethodSignature);
        }
        else
        {
            operation.Description = new StringBuilder()
                .AppendLine(operation.Description)
                .AppendLine()
                .Append(WebUtility.HtmlEncode(descriptor.MethodSignature))
                .ToString();
        }
    }

    private static void FixRequestType(OpenApiOperation operation)
    {
        var body = operation.RequestBody;
        if (body == null && operation.Parameters.Count == 0)
        {
            // fix request content type when operation contract has no input
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    { ProtocolConstants.MediaTypeNameSwaggerRequest, new OpenApiMediaType() }
                }
            };
        }
        else if (body != null && body.Content.TryGetValue(Multipart, out var mediaType))
        {
            body.Content.Remove(Multipart);
            body.Content.Add(ProtocolConstants.MediaTypeNameSwaggerRequest, mediaType);
        }
    }

    private static void AddResponseHeaders(
        OpenApiOperation operation,
        ApiResponseType responseType,
        JsonSchemaResolver schemaResolver,
        JsonSchemaGenerator schemaGenerator)
    {
        var headers = (responseType.ModelMetadata as ApiModelMetadata)?.Headers;
        if (headers == null || headers.Length == 0)
        {
            return;
        }

        if (!operation.Responses.TryGetValue(responseType.StatusCode.ToString(), out var response))
        {
            return;
        }

        for (var i = 0; i < headers.Length; i++)
        {
            var metadata = headers[i];
            var headerType = metadata.Type.ToContextualType();
            var schema = schemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(headerType, schemaResolver);
            response.Headers.Add(metadata.Name, new OpenApiHeader { Schema = schema });
        }
    }
}