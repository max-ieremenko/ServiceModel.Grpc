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

using Grpc.Core;

namespace ServiceModel.Grpc.Internal;

internal sealed class GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse> : Method<TRequest, TResponse>
{
    public GrpcMethod(
        MethodType type,
        string serviceName,
        string name,
        Marshaller<TRequestHeader>? requestHeaderMarshaller,
        Marshaller<TRequest> requestMarshaller,
        Marshaller<TResponseHeader>? responseHeaderMarshaller,
        Marshaller<TResponse> responseMarshaller)
        : base(type, serviceName, name, requestMarshaller, responseMarshaller)
    {
        RequestHeaderMarshaller = requestHeaderMarshaller;
        ResponseHeaderMarshaller = responseHeaderMarshaller;
    }

    public Marshaller<TRequestHeader>? RequestHeaderMarshaller { get; }

    public Marshaller<TResponseHeader>? ResponseHeaderMarshaller { get; }
}