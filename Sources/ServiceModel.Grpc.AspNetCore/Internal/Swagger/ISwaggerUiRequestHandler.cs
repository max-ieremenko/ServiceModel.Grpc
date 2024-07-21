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
using Microsoft.AspNetCore.Http;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger;

internal interface ISwaggerUiRequestHandler
{
    Task<byte[]> ReadRequestMessageAsync(
        PipeReader bodyReader,
        IMarshallerFactory marshallerFactory,
        IOperationDescriptor descriptor,
        CancellationToken token);

    void AppendResponseTrailers(
        IHeaderDictionary responseHeaders,
        IHeaderDictionary? trailers);

    Task WriteResponseMessageAsync(
        MemoryStream original,
        PipeWriter bodyWriter,
        IMarshallerFactory marshallerFactory,
        IOperationDescriptor descriptor,
        CancellationToken token);

    Task WriteResponseErrorAsync(
        RpcException error,
        PipeWriter bodyWriter,
        CancellationToken token);
}