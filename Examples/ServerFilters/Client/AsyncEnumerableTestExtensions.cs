using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Client;

internal static class AsyncEnumerableTestExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source, [EnumeratorCancellation] CancellationToken token)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (var i in source)
        {
            yield return i;
        }
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken token)
    {
        var result = new List<T>();

        await foreach (var i in source.WithCancellation(token).ConfigureAwait(false))
        {
            result.Add(i);
        }

        return result;
    }
}