using System.Threading;

namespace Server.Services;

internal sealed class IncrementingCounter
{
    private long _count;

    public long Count => Interlocked.Read(ref _count);
     
    public long Increment(int amount)
    {
        return Interlocked.Add(ref _count, amount);
    }
}