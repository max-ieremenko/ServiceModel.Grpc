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
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class HeadersService : IHeadersService
    {
        public string GetRequestHeader(string headerName, CallContext context)
        {
            var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == headerName);
            return header?.Value;
        }

        public async Task WriteResponseHeader(string headerName, string headerValue, CallContext context)
        {
            await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { headerName, headerValue }
                });
        }

        public async IAsyncEnumerable<int> ServerStreamingWriteResponseHeader(string headerName, string headerValue, CallContext context = default)
        {
            ServerCallContext serverContext = context;
            await serverContext.WriteResponseHeadersAsync(new Metadata
                {
                    { headerName, headerValue }
                });

            foreach (var i in Enumerable.Range(1, 10))
            {
                await Task.Delay(0);
                yield return i;
            }
        }

        public async Task<string> ClientStreaming(IAsyncEnumerable<int> values, CallContext context = default)
        {
            var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == "h1");

            var list = await values.ToListAsync();

            await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { "h1", header.Value + list.Count }
                });

            return header.Value;
        }

        public async IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> values, CallContext context = default)
        {
            var header = context.ServerCallContext.RequestHeaders.FirstOrDefault(i => i.Key == "h1");

            var list = await values.ToListAsync();

            await context.ServerCallContext.WriteResponseHeadersAsync(new Metadata
                {
                    { "h1", header.Value + list.Count }
                });

            yield return header.Value;
        }
    }
}
