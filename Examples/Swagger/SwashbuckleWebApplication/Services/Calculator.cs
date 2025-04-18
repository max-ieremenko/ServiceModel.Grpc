using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace SwashbuckleWebApplication.Services;

/// <summary>
/// Provides methods for mathematical functions.
/// </summary>
internal sealed class Calculator : ICalculator
{
    public Task<int> GetRandomNumber()
    {
        return Task.FromResult(new Random(DateTime.Now.Millisecond).Next());
    }

    public Task<long> Sum(long x, int y, int z, CancellationToken token)
    {
        return Task.FromResult(x + y + z);
    }

    public ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token)
    {
        var multiplicationResult = DoMultiplication(values, multiplier, token);
        return new ValueTask<(int, IAsyncEnumerable<int>)>((multiplier, multiplicationResult));
    }

    private static async IAsyncEnumerable<int> DoMultiplication(IAsyncEnumerable<int> values, int multiplier, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var value in values.WithCancellation(token))
        {
            yield return value * multiplier;
        }
    }
}