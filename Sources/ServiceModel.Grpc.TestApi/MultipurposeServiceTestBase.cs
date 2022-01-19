// <copyright>
// Copyright 2020-2022 Max Ieremenko
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

using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class MultipurposeServiceTestBase
    {
        protected IMultipurposeService DomainService { get; set; } = null!;

        [Test]
        public void ConcatB()
        {
            var context = new CallOptions(new Metadata
            {
                { "value", "b" }
            });

            var actual = DomainService.Concat("a", context);

            actual.ShouldBe("ab");
        }

        [Test]
        public async Task ConcatBAsync()
        {
            var context = new CallOptions(new Metadata
            {
                { "value", "b" }
            });

            var actual = await DomainService.ConcatAsync("a", context).ConfigureAwait(false);

            actual.ShouldBe("ab");
        }

        [Test]
        public async Task Sum5ValuesAsync()
        {
            var actual = await DomainService.Sum5ValuesAsync(1, 2, 3, 4, 5, default).ConfigureAwait(false);

            actual.ShouldBe(15);
        }

        [Test]
        public void BlockingCall()
        {
            var actual = DomainService.BlockingCall(10, "dummy", default);

            actual.ShouldBe("dummy10");
        }

        [Test]
        public async Task BlockingCallAsync()
        {
            var actual = await DomainService.BlockingCallAsync(default, 10, "dummy").ConfigureAwait(false);

            actual.ShouldBe("dummy10");
        }

        [Test]
        public async Task RepeatValue()
        {
            var actual = await DomainService.RepeatValue("a", 3).ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { "a", "a", "a" });
        }

        [Test]
        public async Task ServerStreamingStopReading()
        {
            var stream = DomainService.RepeatValue("a", int.MaxValue);
            await foreach (var value in stream.ConfigureAwait(false))
            {
                value.ShouldBe("a");
                break;
            }
        }

        [Test]
        public async Task RepeatValueAsync()
        {
            var source = await DomainService.RepeatValueAsync("a", 3).ConfigureAwait(false);
            var actual = await source.ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { "a", "a", "a" });
        }

        [Test]
        public async Task GenerateArraysAsync()
        {
            var (totalItemsCount, arrays) = await DomainService.GenerateArraysAsync(10, 5).ConfigureAwait(false);

            totalItemsCount.ShouldBe(10 * 5);

            var arraysCount = 0;
            await foreach (var array in arrays.ConfigureAwait(false))
            {
                arraysCount++;
                array.ShouldBe(Enumerable.Range(0, 10).Select(i => (byte)i).ToArray());
            }

            arraysCount.ShouldBe(5);
        }

        [Test]
        public async Task SumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.SumValues(values).ConfigureAwait(false);

            actual.ShouldBe(6);
        }

        [Test]
        public async Task MultiplyByAndSumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.MultiplyByAndSumValues(values, 2, null).ConfigureAwait(false);

            actual.ShouldBe(12);
        }

        [Test]
        public async Task ClientStreamingStopReading()
        {
            var values = Enumerable.Range(1, int.MaxValue).AsAsyncEnumerable();

            var actual = await DomainService.MultiplyByAndSumValues(values, 2, 1).ConfigureAwait(false);

            actual.ShouldBe(2);
        }

        [Test]
        public async Task ConvertValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.ConvertValues(values).ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { "1", "2", "3" });
        }

        [Test]
        public async Task MultiplyBy()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.MultiplyBy(values, 2, null).ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { 2, 4, 6 });
        }

        [Test]
        public async Task DuplexStreamingServerStopReading()
        {
            var values = Enumerable.Range(1, int.MaxValue).AsAsyncEnumerable();

            var actual = await DomainService.MultiplyBy(values, 2, 1).ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { 2 });
        }

        [Test]
        public async Task DuplexStreamingClientStopReading()
        {
            var values = Enumerable.Range(1, int.MaxValue).AsAsyncEnumerable();

            var response = DomainService.MultiplyBy(values, 2, null);
            await foreach (var value in response.ConfigureAwait(false))
            {
                value.ShouldBe(2);
                break;
            }
        }

        [Test]
        public async Task MultiplyByAsync()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var source = await DomainService.MultiplyByAsync(values, 2).ConfigureAwait(false);
            var actual = await source.ToListAsync().ConfigureAwait(false);

            actual.ShouldBe(new[] { 2, 4, 6 });
        }

        [Test]
        public async Task GreetAsync()
        {
            var names = new[] { "world", "grpc", "X" }.AsAsyncEnumerable();

            var (stream, greeting) = await DomainService.GreetAsync(names, "Hello").ConfigureAwait(false);

            greeting.ShouldBe("Hello");

            var greetings = await stream.ToListAsync().ConfigureAwait(false);
            greetings.ShouldBe(new[] { "Hello world", "Hello grpc", "Hello X" });
        }
    }
}
