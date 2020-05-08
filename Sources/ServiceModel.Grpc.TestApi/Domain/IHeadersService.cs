// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi.Domain
{
    [ServiceContract]
    public interface IHeadersService
    {
        [OperationContract]
        string GetRequestHeader(string headerName, CallContext context = default);

        [OperationContract]
        Task WriteResponseHeader(string headerName, string headerValue, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<int> ServerStreamingWriteResponseHeader(string headerName, string headerValue, CallContext context = default);

        [OperationContract]
        Task<string> ClientStreaming(IAsyncEnumerable<int> values, CallContext context = default);

        [OperationContract]
        IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> values, CallContext context = default);
    }
}
