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

            var actual = await DomainService.ConcatAsync("a", context);

            actual.ShouldBe("ab");
        }

        [Test]
        public async Task Sum5ValuesAsync()
        {
            var actual = await DomainService.Sum5ValuesAsync(1, 2, 3, 4, 5, default);

            actual.ShouldBe(15);
        }

        [Test]
        public async Task RepeatValue()
        {
            var actual = await DomainService.RepeatValue("a", 3).ToListAsync();

            actual.ShouldBe(new[] { "a", "a", "a" });
        }

        [Test]
        public async Task RepeatValueAsync()
        {
            var source = await DomainService.RepeatValueAsync("a", 3);
            var actual = await source.ToListAsync();

            actual.ShouldBe(new[] { "a", "a", "a" });
        }

        [Test]
        public async Task SumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.SumValues(values);

            actual.ShouldBe(6);
        }

        [Test]
        public async Task MultiplyByAndSumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.MultiplyByAndSumValues(values, 2);

            actual.ShouldBe(12);
        }

        [Test]
        public async Task ConvertValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.ConvertValues(values).ToListAsync();

            actual.ShouldBe(new[] { "1", "2", "3" });
        }

        [Test]
        public async Task MultiplyBy()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await DomainService.MultiplyBy(values, 2).ToListAsync();

            actual.ShouldBe(new[] { 2, 4, 6 });
        }

        [Test]
        public async Task MultiplyByAsync()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var source = await DomainService.MultiplyByAsync(values, 2);
            var actual = await source.ToListAsync();

            actual.ShouldBe(new[] { 2, 4, 6 });
        }
    }
}
