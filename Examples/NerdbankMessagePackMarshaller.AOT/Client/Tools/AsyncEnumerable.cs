using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Tools;

internal static class AsyncEnumerable
{
    public static async IAsyncEnumerable<T> AsAsync<T>(params T[] values)
    {
        await Task.CompletedTask;

        foreach (var value in values)
        {
            yield return value;
        }
    }

    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        await foreach (var item in enumerable.WithCancellation(cancellationToken))
        {
            result.Add(item);
        }

        return result.ToArray();
    }
}