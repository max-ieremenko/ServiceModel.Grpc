// <copyright>
// Copyright 2020-2021 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi;

public static class AsyncEnumerableTestExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var i in source)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield return i;
        }
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var result = new List<T>();

        await foreach (var i in source.ConfigureAwait(false))
        {
            result.Add(i);
        }

        return result;
    }

    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this ChannelReader<T> reader, CancellationToken token)
    {
#if NET6_0_OR_GREATER
        return reader.ReadAllAsync(token);
#else
            return ReadAllAsync(reader, token);
#endif
    }

    private static async IAsyncEnumerable<T> ReadAllAsync<T>(ChannelReader<T> reader, [EnumeratorCancellation] CancellationToken token)
    {
        while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }
}