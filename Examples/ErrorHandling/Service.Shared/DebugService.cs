using System;
using System.Threading.Tasks;
using Contract;

namespace Service.Shared;

public sealed class DebugService : IDebugService
{
    public Task ThrowApplicationException(string message)
    {
        throw new ApplicationException(message);
    }

    public Task ThrowRandomException(string message)
    {
        var randomValue = new Random(DateTime.Now.Millisecond).Next(0, 2);
        if (randomValue == 0)
        {
            throw new InvalidOperationException(message);
        }

        throw new NotSupportedException(message);
    }
}