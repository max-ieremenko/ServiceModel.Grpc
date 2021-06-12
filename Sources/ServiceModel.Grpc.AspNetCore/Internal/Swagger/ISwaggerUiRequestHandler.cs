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

using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace ServiceModel.Grpc.AspNetCore.Internal.Swagger
{
    internal interface ISwaggerUiRequestHandler
    {
        Task<byte[]> ReadRequestMessageAsync(
            PipeReader bodyReader,
            IList<string> orderedParameterNames,
            IMethod method,
            CancellationToken token);

        void AppendResponseTrailers(
            IHeaderDictionary responseHeaders,
            IHeaderDictionary? trailers);

        Task WriteResponseMessageAsync(
            MemoryStream original,
            PipeWriter bodyWriter,
            IMethod method,
            CancellationToken token);

        Task WriteResponseErrorAsync(
            RpcException error,
            PipeWriter bodyWriter,
            CancellationToken token);
    }
}
