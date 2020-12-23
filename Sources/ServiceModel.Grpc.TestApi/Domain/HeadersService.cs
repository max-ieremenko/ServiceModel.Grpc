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
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class HeadersService : IHeadersService
    {
        public const string DefaultHeaderName = "default-Header";
        public const string DefaultHeaderValue = "default-Header value";

        public const string CallHeaderName = "call-Header";
        public const string CallHeaderValue = "call-Header value";

        public const string CallTrailerName = "call-Trailer";
        public const string CallTrailerValue = "call-Trailer value";

        public void UnaryCall(CallContext context)
        {
            WriteResponseHeadersAsync(context).Wait();
            WriteResponseTrailers(context);
        }

        public async Task UnaryCallAsync(CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);
            WriteResponseTrailers(context);
        }

        public async IAsyncEnumerable<int> ServerStreamingCall(CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);

            foreach (var i in Enumerable.Range(1, 10))
            {
                await Task.Delay(0);
                yield return i;
            }

            WriteResponseTrailers(context);
        }

        public async Task<IAsyncEnumerable<int>> ServerStreamingCallAsync(CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);

            WriteResponseTrailers(context);

            return Enumerable.Range(1, 10).AsAsyncEnumerable();
        }

        public async Task ClientStreamingCall(IAsyncEnumerable<int> stream, CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);

            var list = await stream.ToListAsync().ConfigureAwait(false);
            list.Count.ShouldBe(10);

            WriteResponseTrailers(context);
        }

        public async IAsyncEnumerable<int> DuplexStreamingCall(IAsyncEnumerable<int> stream, CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);

            var list = await stream.ToListAsync().ConfigureAwait(false);
            list.Count.ShouldBe(10);

            foreach (var i in Enumerable.Range(1, 10))
            {
                await Task.Delay(0);
                yield return i;
            }

            WriteResponseTrailers(context);
        }

        public async Task<IAsyncEnumerable<int>> DuplexStreamingCallAsync(IAsyncEnumerable<int> stream, CallContext context)
        {
            await WriteResponseHeadersAsync(context).ConfigureAwait(false);

            var list = await stream.ToListAsync().ConfigureAwait(false);
            list.Count.ShouldBe(10);

            WriteResponseTrailers(context);

            return Enumerable.Range(1, 10).AsAsyncEnumerable();
        }

        private static async Task WriteResponseHeadersAsync(CallContext context)
        {
            ServerCallContext serverContext = context!;

            var defaultHeader = serverContext.RequestHeaders.FindHeader(DefaultHeaderName, false);
            defaultHeader.ShouldNotBeNull();
            defaultHeader.Value.ShouldBe(DefaultHeaderValue);

            var callHeader = serverContext.RequestHeaders.FindHeader(CallHeaderName, false);
            callHeader.ShouldNotBeNull();
            callHeader.Value.ShouldBe(CallHeaderValue);

            await serverContext
                .WriteResponseHeadersAsync(new Metadata
                {
                    { DefaultHeaderName, DefaultHeaderValue },
                    { CallHeaderName, CallHeaderValue }
                })
                .ConfigureAwait(false);
        }

        private static void WriteResponseTrailers(CallContext context)
        {
            ServerCallContext serverContext = context!;
            serverContext.ResponseTrailers.Add(CallTrailerName, CallTrailerValue);
        }
    }
}
