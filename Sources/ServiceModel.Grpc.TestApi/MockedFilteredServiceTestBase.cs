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
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceModel.Grpc.Filters;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class MockedFilteredServiceTestBase
    {
        protected IFilteredService DomainService { get; set; } = null!;

        [Test]
        public async Task UnaryAsync()
        {
            var track = await DomainService.UnaryAsync(new[] { "client-call" }).ConfigureAwait(false);

            track.ShouldBeNull();
        }

        [Test]
        public async Task ClientStreamAsync()
        {
            var stream = new[] { 1, 2, 3 }.AsAsyncEnumerable();
            var track = await DomainService.ClientStreamAsync(stream, new[] { "client-call" }).ConfigureAwait(false);

            track.ShouldBeNull();
        }

        [Test]
        public async Task ServerStreamAsync()
        {
            var (stream, track) = await DomainService.ServerStreamAsync(new[] { "client-call" }).ConfigureAwait(false);

            var data = await stream.ToListAsync().ConfigureAwait(false);
            data.ShouldBeEmpty();
            track.ShouldBeNull();
        }

        [Test]
        public async Task DuplexStreamAsync()
        {
            var inputStream = new[] { 1, 2, 3 }.AsAsyncEnumerable();
            var (outStream, track) = await DomainService.DuplexStreamAsync(inputStream, new[] { "client-call" }).ConfigureAwait(false);

            var data = await outStream.ToListAsync().ConfigureAwait(false);
            data.ShouldBeEmpty();
            track.ShouldBeNull();
        }

        protected sealed class MockServerFilter : IServerFilter
        {
            public async ValueTask InvokeAsync(IServerFilterContext context, Func<ValueTask> next)
            {
                var clientStream = (IAsyncEnumerable<int>?)context.Request.Stream;
                if (clientStream != null)
                {
                    await foreach (var i in clientStream.ConfigureAwait(false))
                    {
                    }
                }
            }
        }
    }
}
