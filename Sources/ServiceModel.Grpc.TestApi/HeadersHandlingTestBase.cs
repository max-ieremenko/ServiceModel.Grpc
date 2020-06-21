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
using NUnit.Framework;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class HeadersHandlingTestBase
    {
        protected Metadata DefaultMetadata { get; } = new Metadata
        {
            { "defaultHeader", "defaultHeader value" }
        };

        protected IHeadersService DomainService { get; set; } = null!;

        [Test]
        public void GetDefaultRequestHeader()
        {
            DomainService.GetRequestHeader(DefaultMetadata[0].Key).ShouldBe(DefaultMetadata[0].Value);
        }

        [Test]
        public void GetRequestHeader()
        {
            var options = new CallOptions(new Metadata
            {
                { "h1", "value 1" }
            });

            DomainService.GetRequestHeader("h1", options).ShouldBe("value 1");
        }

        [Test]
        public async Task WriteResponseHeader()
        {
            var context = new CallContext();

            await DomainService.WriteResponseHeader("h1", "value 1", context);

            var actual = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            actual.ShouldBe(new[] { "value 1" });
        }

        [Test]
        public async Task ServerStreamingWriteResponseHeader()
        {
            var context = new CallContext();

            var response = DomainService.ServerStreamingWriteResponseHeader("h1", "value 1", context);

            context.ResponseHeaders.ShouldBeNull();

            var values = new List<int>();
            await foreach (var i in response)
            {
                context.ResponseHeaders.ShouldNotBeNull();
                values.Add(i);
            }

            var actual = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            actual.ShouldBe(new[] { "value 1" });

            values.Count.ShouldBe(10);
        }

        [Test]
        public async Task ClientStreaming()
        {
            var context = new CallContext(new Metadata
            {
                { "h1", "value " }
            });

            var actual = await DomainService.ClientStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), context);

            context.ResponseHeaders.ShouldNotBeNull();
            actual.ShouldBe("value ");

            var header = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            header.ShouldBe(new[] { "value 2" });
        }

        [Test]
        public async Task DuplexStreaming()
        {
            var context = new CallContext(new Metadata
            {
                { "h1", "value " }
            });

            var response = DomainService.DuplexStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), context);

            context.ResponseHeaders.ShouldBeNull();

            var values = new List<string>();
            await foreach (var i in response)
            {
                context.ResponseHeaders.ShouldNotBeNull();
                values.Add(i);
            }

            var header = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            header.ShouldBe(new[] { "value 2" });

            values.ShouldBe(new[] { "value " });
        }
    }
}
