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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class MultipurposeService : IMultipurposeService
    {
        public string Concat(string value, CallContext? context)
        {
            context?.ServerCallContext.ShouldNotBeNull();

            return value + context!.ServerCallContext!.RequestHeaders.First(i => i.Key == "value").Value;
        }

        public Task<string> ConcatAsync(string value, CallContext? context)
        {
            context?.ServerCallContext.ShouldNotBeNull();

            var result = value + context!.ServerCallContext!.RequestHeaders.First(i => i.Key == "value").Value;
            return Task.FromResult(result);
        }

        public ValueTask<long> Sum5ValuesAsync(long x1, int x2, int x3, int x4, int x5, CancellationToken? token)
        {
            token.ShouldNotBeNull();
            token.Value.CanBeCanceled.ShouldBeTrue();

            return new ValueTask<long>(x1 + x2 + x3 + x4 + x5);
        }

        public string BlockingCall(int x, string y, CancellationToken token) => throw new NotSupportedException();

        public Task<string> BlockingCallAsync(CancellationToken token, int x, string y)
        {
            return Task.FromResult(y + x);
        }

        public async IAsyncEnumerable<string> RepeatValue(string value, int count, CallContext? context)
        {
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(300).ConfigureAwait(false);
                yield return value;
            }
        }

        public async Task<IAsyncEnumerable<string>> RepeatValueAsync(string value, int count, CallContext? context)
        {
            await Task.Delay(100).ConfigureAwait(false);
            return RepeatValue(value, count, context);
        }

        public ValueTask<(int TotalItemsCount, IAsyncEnumerable<byte[]> Arrays)> GenerateArraysAsync(int arrayLength, int count, CancellationToken token)
        {
            var totalItemsCount = arrayLength * count;
            var arrays = GenerateArrays(arrayLength, count, token);
            return new ValueTask<(int, IAsyncEnumerable<byte[]>)>((totalItemsCount, arrays));
        }

        public async Task<long> SumValues(IAsyncEnumerable<int> values, CallContext? context)
        {
            var result = 0;
            await foreach (var i in values.WithCancellation(context!.ServerCallContext!.CancellationToken).ConfigureAwait(false))
            {
                result += i;
            }

            return result;
        }

        public async Task<long> MultiplyByAndSumValues(IAsyncEnumerable<int> values, int multiplier, int? valuesCount, CallContext? context)
        {
            var result = 0;

            var counter = 0;
            await foreach (var i in values.WithCancellation(context!.ServerCallContext!.CancellationToken).ConfigureAwait(false))
            {
                result += i * multiplier;

                counter++;
                if (counter == valuesCount)
                {
                    break;
                }
            }

            return result;
        }

        public async IAsyncEnumerable<string> ConvertValues(IAsyncEnumerable<int> values, CallContext? context)
        {
            await foreach (var i in values.WithCancellation(context!.ServerCallContext!.CancellationToken).ConfigureAwait(false))
            {
                yield return i.ToString();
            }
        }

        public async IAsyncEnumerable<int> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, int? valuesCount, CallContext? context)
        {
            var counter = 0;
            await foreach (var i in values.WithCancellation(context!.ServerCallContext!.CancellationToken).ConfigureAwait(false))
            {
                yield return i * multiplier;

                counter++;
                if (counter == valuesCount)
                {
                    yield break;
                }
            }
        }

        public async ValueTask<IAsyncEnumerable<int>> MultiplyByAsync(IAsyncEnumerable<int> values, int multiplier, CallContext? context)
        {
            await Task.Delay(100).ConfigureAwait(false);
            return MultiplyBy(values, multiplier, null, context);
        }

        public ValueTask<(IAsyncEnumerable<string> Greetings, string Greeting)> GreetAsync(IAsyncEnumerable<string> names, string greeting, CancellationToken token)
        {
            var greetings = Greet(names, greeting, token);
            return new ValueTask<(IAsyncEnumerable<string>, string)>((greetings, greeting));
        }

        private static async IAsyncEnumerable<byte[]> GenerateArrays(int arrayLength, int count, [EnumeratorCancellation] CancellationToken token)
        {
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(1, token).ConfigureAwait(false);

                var array = new byte[arrayLength];
                for (var j = 0; j < arrayLength; j++)
                {
                    array[j] = (byte)j;
                }

                yield return array;
            }
        }

        private static async IAsyncEnumerable<string> Greet(IAsyncEnumerable<string> names, string greeting, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var name in names.WithCancellation(token).ConfigureAwait(false))
            {
                yield return greeting + " " + name;
            }
        }
    }
}
