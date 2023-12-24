using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace WebApplication;

internal sealed class Calculator : ICalculator
{
    public async Task<long> Sum(int x, int y, CancellationToken token)
    {
        await Task.Delay(100, token);

        return (long)x + y;
    }
}