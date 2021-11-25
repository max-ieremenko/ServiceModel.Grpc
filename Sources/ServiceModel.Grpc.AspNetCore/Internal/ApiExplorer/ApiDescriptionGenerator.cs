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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using ServiceModel.Grpc.Internal;
#if NET6_0
using RouteValuesType = System.Collections.Generic.Dictionary<string, string?>;
#else
using RouteValuesType = System.Collections.Generic.Dictionary<string, string>;
#endif

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer
{
    internal static class ApiDescriptionGenerator
    {
        public static ApiDescription? TryCreateApiDescription(RouteEndpoint endpoint)
        {
            var metadata = endpoint.Metadata.GetMetadata<GrpcMethodMetadata>();
            if (metadata == null)
            {
                return null;
            }

            var marker = FindServiceModelGrpcMarker(endpoint.Metadata);
            if (marker == null)
            {
                return null;
            }

            var operation = new OperationDescription(
                metadata.Method.ServiceName,
                metadata.Method.Name,
                new MessageAssembler(marker.ContractMethodDefinition));

            return CreateApiDescription(endpoint, metadata, operation);
        }

        internal static IEnumerable<ParameterInfo> GetRequestHeaderParameters(MessageAssembler message)
        {
            for (var i = 0; i < message.HeaderRequestTypeInput.Length; i++)
            {
                yield return message.Parameters[message.HeaderRequestTypeInput[i]];
            }
        }

        internal static IEnumerable<ParameterInfo> GetRequestParameters(MessageAssembler message)
        {
            for (var i = 0; i < message.RequestTypeInput.Length; i++)
            {
                yield return message.Parameters[message.RequestTypeInput[i]];
            }
        }

        internal static (Type? Type, ParameterInfo Parameter) GetResponseType(MessageAssembler message)
        {
            // do not return typeof(void) => Swashbuckle schema generation error
            var arguments = message.ResponseType.GetGenericArguments();
            var responseType = arguments.Length == 0 ? null : arguments[0];
            if (message.OperationType == MethodType.ServerStreaming || message.OperationType == MethodType.DuplexStreaming)
            {
                responseType = typeof(IAsyncEnumerable<>).MakeGenericType(responseType!);
            }

            return (responseType, message.Operation.ReturnParameter);
        }

        internal static (Type Type, string Name)[] GetResponseHeaderParameters(MessageAssembler message)
        {
            var result = new (Type Type, string Name)[message.HeaderResponseTypeInput.Length];
            if (result.Length > 0)
            {
                var types = message.HeaderResponseType!.GetGenericArguments();
                var names = message.GetResponseHeaderNames();

                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = (types[i]!, names[i]);
                }
            }

            return result;
        }

        private static ApiDescription CreateApiDescription(
            RouteEndpoint endpoint,
            GrpcMethodMetadata metadata,
            OperationDescription operation)
        {
            var serviceInstanceMethod = ReflectionTools.ImplementationOfMethod(
                metadata.ServiceType,
                operation.Message.Operation.DeclaringType!,
                operation.Message.Operation);

            var descriptor = new GrpcActionDescriptor
            {
                MethodInfo = serviceInstanceMethod,
                ControllerTypeInfo = metadata.ServiceType.GetTypeInfo(),
                ActionName = metadata.Method.Name,
                ControllerName = metadata.Method.ServiceName,
                RouteValues = new RouteValuesType
                {
                    ["controller"] = metadata.Method.ServiceName
                },
                MethodType = metadata.Method.Type,
                MethodSignature = GetSignature(operation.Message, metadata.Method.Name)
            };

            var description = new ApiDescription
            {
                HttpMethod = HttpMethods.Post,
                ActionDescriptor = descriptor,
                RelativePath = ProtocolConstants.NormalizeRelativePath(endpoint.RoutePattern.RawText!)
            };

            AddRequest(description, operation.Message);
            AddResponse(description, operation.Message);

            return description;
        }

        private static void AddRequest(ApiDescription description, MessageAssembler message)
        {
            description.SupportedRequestFormats.Add(new ApiRequestFormat
            {
                MediaType = ProtocolConstants.MediaTypeNameSwaggerRequest
            });

            foreach (var parameter in GetRequestHeaderParameters(message))
            {
                description.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = parameter.Name!,
                    ModelMetadata = ApiModelMetadata.ForParameter(parameter),
                    Source = BindingSource.Header,
                    Type = parameter.ParameterType
                });
            }

            foreach (var parameter in GetRequestParameters(message))
            {
                description.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = parameter.Name!,
                    ModelMetadata = ApiModelMetadata.ForParameter(parameter),
                    Source = BindingSource.Form,
                    Type = parameter.ParameterType
                });
            }
        }

        private static void AddResponse(ApiDescription description, MessageAssembler message)
        {
            var (responseType, parameter) = GetResponseType(message);
            ApiModelMetadata? model = null;
            if (responseType != null)
            {
                model = ApiModelMetadata.ForParameter(parameter, responseType);
                model.Headers = GetResponseHeaderParameters(message);
            }

            description.SupportedResponseTypes.Add(new ApiResponseType
            {
                ApiResponseFormats =
                {
                    new ApiResponseFormat { MediaType = ProtocolConstants.MediaTypeNameSwaggerResponse }
                },
                ModelMetadata = model,
                Type = responseType,
                StatusCode = (int)HttpStatusCode.OK
            });
        }

        private static ServiceModelGrpcMarker? FindServiceModelGrpcMarker(IReadOnlyList<object> metadata)
        {
            for (var i = 0; i < metadata.Count; i++)
            {
                if (metadata[i] is ServiceModelGrpcMarker marker)
                {
                    return marker;
                }
            }

            return null;
        }

        private static string GetSignature(MessageAssembler message, string actionName)
        {
            var result = new StringBuilder();

            var response = GetResponseType(message);
            var responseHeader = GetResponseHeaderParameters(message);
            if (response.Type == null)
            {
                result.Append("void ");
            }
            else
            {
                if (responseHeader.Length > 0)
                {
                    result.Append("(");
                }

                result.Append(response.Type.GetUserFriendlyName());

                if (responseHeader.Length > 0)
                {
                    for (var i = 0; i < responseHeader.Length; i++)
                    {
                        var header = responseHeader[i];
                        result
                            .Append(", ")
                            .Append(header.Type.GetUserFriendlyName())
                            .Append(" ")
                            .Append(header.Name);
                    }

                    result.Append(")");
                }
            }

            result
                .Append(" ")
                .Append(actionName)
                .Append("(");

            var index = 0;
            foreach (var parameter in GetRequestParameters(message).Concat(GetRequestHeaderParameters(message)))
            {
                if (index > 0)
                {
                    result.Append(", ");
                }

                index++;
                result
                    .Append(parameter.ParameterType.GetUserFriendlyName())
                    .Append(" ")
                    .Append(parameter.Name);
            }

            result.Append(")");
            return result.ToString();
        }
    }
}
