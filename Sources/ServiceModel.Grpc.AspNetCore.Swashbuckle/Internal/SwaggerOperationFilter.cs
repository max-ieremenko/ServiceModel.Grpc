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

using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle.Internal
{
    internal sealed class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!(context.ApiDescription.ActionDescriptor is GrpcActionDescriptor descriptor))
            {
                return;
            }

            UpdateSummary(operation, descriptor);
            UpdateDescription(operation, descriptor);

            for (var i = 0; i < context.ApiDescription.SupportedResponseTypes.Count; i++)
            {
                AddResponseHeaders(operation, context.ApiDescription.SupportedResponseTypes[i], context.SchemaRepository, context.SchemaGenerator);
            }
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
                operation.Description = descriptor.MethodSignature;
            }
            else
            {
                operation.Description = new StringBuilder(operation.Description.Length + 4 + descriptor.MethodSignature)
                    .AppendLine(operation.Description)
                    .AppendLine()
                    .Append(descriptor.MethodSignature)
                    .ToString();
            }
        }

        private static void AddResponseHeaders(
            OpenApiOperation operation,
            ApiResponseType responseType,
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator)
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
                var schema = schemaGenerator.GenerateSchema(metadata.Type, schemaRepository);
                response.Headers.Add(metadata.Name, new OpenApiHeader { Schema = schema });
            }
        }
    }
}
