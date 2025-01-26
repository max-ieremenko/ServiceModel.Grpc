using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract;

namespace Server.CodeFirst;

internal sealed class CalculatorService : ICalculator
{
    public Task<long> SumAsync(long x, int y, int z)
    {
        return Task.FromResult(x + y + z);
    }

    public async Task<long> SumValuesAsync(IAsyncEnumerable<int> values)
    {
        var result = 0L;

        await foreach (var i in values)
        {
            result += i;
        }

        return result;
    }

    public IAsyncEnumerable<int> Range(int start, int count)
    {
        return Enumerable.Range(start, count).AsAsyncEnumerable();
    }

    public async IAsyncEnumerable<int> MultiplyBy2(IAsyncEnumerable<int> values)
    {
        await foreach (var i in values)
        {
            yield return i * 2;
        }
    }
}