using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Service.Filters;

namespace Service
{
    public sealed class Calculator : ICalculator
    {
        [SumAsyncFilter(100)]
        public ValueTask<int> SumAsync(int x, int y, CancellationToken token)
        {
            // see SumAsyncServerFilter
            throw new NotImplementedException();
        }

        [ValidateParameterFilter(100)]
        public ValueTask<DivideByResult> DivideByAsync(int value, [Not(0), GreaterThan(10)] int divider, CancellationToken token)
        {
            var result = new DivideByResult
            {
                IsSuccess = true,
                Result = value / divider
            };

            return new ValueTask<DivideByResult>(result);
        }

        [HackMultiplyFilterBy(100)]
        public ValueTask<(IAsyncEnumerable<int> Values, int Multiplier)> MultiplyByAsync(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
        {
            var output = DoMultiplyBy(values, multiplier, token);
            return new ValueTask<(IAsyncEnumerable<int>, int)>((output, multiplier));
        }

        private async IAsyncEnumerable<int> DoMultiplyBy(IAsyncEnumerable<int> values, int multiplier, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var value in values.WithCancellation(token).ConfigureAwait(false))
            {
                yield return value * multiplier;
            }
        }
    }
}
