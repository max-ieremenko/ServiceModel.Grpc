using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace WebApplication;

internal sealed class RandomNumberGenerator : IRandomNumberGenerator
{
    public async Task<int> NextInt32(CancellationToken token = default)
    {
        await Task.Delay(100, token);

        return Random.Shared.Next();
    }
}