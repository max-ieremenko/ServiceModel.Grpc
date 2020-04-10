using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi
{
    public static class AsyncEnumerableTestExtensions
    {
        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            foreach (var i in source)
            {
                await Task.CompletedTask;
                yield return i;
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach (var i in source)
            {
                result.Add(i);
            }

            return result;
        }
    }
}
