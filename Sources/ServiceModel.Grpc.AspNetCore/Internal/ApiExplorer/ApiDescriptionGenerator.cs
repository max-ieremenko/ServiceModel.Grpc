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
using System.Reflection;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.ApiExplorer;

internal static class ApiDescriptionGenerator
{
    public static ApiDescription? TryCreateApiDescription(RouteEndpoint endpoint)
    {
        var metadata = endpoint.Metadata.GetMetadata<GrpcMethodMetadata>();
        if (metadata == null)
        {
            return null;
        }

        var marker = endpoint.Metadata.GetMetadata<ServiceModelGrpcMarker>();
        if (marker == null)
        {
            return null;
        }

        var requestParameters = GetRequestParameters(marker.Descriptor);
        var response = GetResponseType(marker.Descriptor);
        var responseHeaderParameters = GetResponseHeaderParameters(marker.Descriptor);

        var descriptor = new GrpcActionDescriptor
        {
            MethodInfo = marker.Descriptor.GetContractMethod(),
            ControllerTypeInfo = metadata.ServiceType.GetTypeInfo(),
            ActionName = metadata.Method.Name,
            ControllerName = metadata.Method.ServiceName,
            RouteValues = new Dictionary<string, string?>(1)
            {
                ["controller"] = metadata.Method.ServiceName
            },
            MethodType = metadata.Method.Type,
            MethodSignature = MethodSignatureBuilder.Build(metadata.Method.Name, requestParameters, response.Type, responseHeaderParameters),
            EndpointMetadata = endpoint.Metadata.ToArray()
        };

        var description = new ApiDescription
        {
            HttpMethod = HttpMethods.Post,
            ActionDescriptor = descriptor,
            RelativePath = ProtocolConstants.NormalizeRelativePath(endpoint.RoutePattern.RawText!)
        };

        AddRequest(description, requestParameters);
        AddResponse(description, response.Type, response.Parameter, responseHeaderParameters);

        return description;
    }

    internal static (BindingSource Source, ParameterInfo Parameter)[] GetRequestParameters(IOperationDescriptor descriptor)
    {
        var headerIndices = descriptor.GetRequestHeaderParameters();
        var indices = descriptor.GetRequestParameters();
        if (indices.Length == 0 && headerIndices.Length == 0)
        {
            return [];
        }

        var parameters = descriptor.GetContractMethod().GetParameters();
        var result = new (BindingSource Source, ParameterInfo Parameter)[headerIndices.Length + indices.Length];

        for (var i = 0; i < headerIndices.Length; i++)
        {
            result[i] = (BindingSource.Header, parameters[headerIndices[i]]);
        }

        for (var i = 0; i < indices.Length; i++)
        {
            result[i + headerIndices.Length] = (BindingSource.Form, parameters[indices[i]]);
        }

        return result;
    }

    internal static (Type? Type, ParameterInfo Parameter) GetResponseType(IOperationDescriptor descriptor)
    {
        // do not return typeof(void) => Swashbuckle schema generation error
        var stream = descriptor.GetResponseStreamAccessor();

        Type? responseType;
        if (stream == null)
        {
            var response = descriptor.GetResponseAccessor();
            responseType = response.Names.Length == 0 ? null : response.GetValueType(0);
        }
        else
        {
            responseType = stream.GetInstanceType();
        }

        return (responseType, descriptor.GetContractMethod().ReturnParameter);
    }

    internal static (Type Type, string Name)[] GetResponseHeaderParameters(IOperationDescriptor descriptor)
    {
        if (descriptor.GetResponseStreamAccessor() == null)
        {
            return [];
        }

        var response = descriptor.GetResponseAccessor();
        var result = new (Type Type, string Name)[response.Names.Length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = (response.GetValueType(i), response.Names[i]);
        }

        return result;
    }

    private static void AddRequest(ApiDescription description, (BindingSource Source, ParameterInfo Parameter)[] requestParameters)
    {
        description.SupportedRequestFormats.Add(new ApiRequestFormat
        {
            MediaType = ProtocolConstants.MediaTypeNameSwaggerRequest
        });

        foreach (var (source, parameter) in requestParameters)
        {
            description.ParameterDescriptions.Add(new ApiParameterDescription
            {
                Name = parameter.Name!,
                ModelMetadata = ApiModelMetadata.ForParameter(parameter),
                Source = source,
                Type = parameter.ParameterType
            });
        }
    }

    private static void AddResponse(
        ApiDescription description,
        Type? responseType,
        ParameterInfo responseParameter,
        (Type Type, string Name)[] responseHeaderParameters)
    {
        ApiModelMetadata? model = null;
        if (responseType != null)
        {
            model = ApiModelMetadata.ForParameter(responseParameter, responseType);
            model.Headers = responseHeaderParameters;
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
}