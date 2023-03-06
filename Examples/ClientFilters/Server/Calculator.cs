using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Server;

internal sealed class Calculator : ICalculator
{
    public ValueTask<int> SumAsync(int x, int y, CancellationToken token)
    {
        // see SumAsyncClientFilter
        throw new NotImplementedException();
    }

    public int DivideBy(int x, int y) => x / y;

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