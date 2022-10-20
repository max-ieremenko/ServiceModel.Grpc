using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contract;

public static class AsyncEnumerableHelper
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var i in source)
        {
            await Task.Delay(100);
            yield return i;
        }
    }
}