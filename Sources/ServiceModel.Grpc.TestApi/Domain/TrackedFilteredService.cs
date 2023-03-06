// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi.Domain;

[TrackingServerFilter(3, "service")]
public sealed class TrackedFilteredService : IFilteredService
{
    [TrackingServerFilter(4, "method")]
    public IList<string> UnarySync(IList<string> input)
    {
        return new List<string>(input) { "implementation" };
    }

    [TrackingServerFilter(4, "method")]
    public ValueTask<IList<string>> UnaryAsync(IList<string> input)
    {
        return new ValueTask<IList<string>>(UnarySync(input));
    }

    [TrackingServerFilter(4, "method")]
    public async ValueTask<IList<string>> ClientStreamAsync(IAsyncEnumerable<int> stream, IList<string> input)
    {
        var sum = 0;
        await foreach (var i in stream.ConfigureAwait(false))
        {
            sum += i;
        }

        return new List<string>(input) { "implementation " + sum };
    }

    [TrackingServerFilter(4, "method")]
    public IAsyncEnumerable<string> ServerStreamSync(IList<string> input)
    {
        var output = new List<string>(input) { "implementation" };

        return output.AsAsyncEnumerable();
    }

    [TrackingServerFilter(4, "method")]
    public ValueTask<(IAsyncEnumerable<int> Stream, IList<string> Output)> ServerStreamAsync(IList<string> input)
    {
        var stream = new[] { 3, 2, 1 }.AsAsyncEnumerable();
        var output = new List<string>(input) { "implementation" };

        return new ValueTask<(IAsyncEnumerable<int>, IList<string>)>((stream, output));
    }

    [TrackingServerFilter(4, "method")]
    public async ValueTask<(IAsyncEnumerable<int> Stream, IList<string> Output)> DuplexStreamAsync(IAsyncEnumerable<int> stream, IList<string> input)
    {
        var sum = 0;
        await foreach (var i in stream.ConfigureAwait(false))
        {
            sum += i;
        }

        var output = new List<string>(input) { "implementation " + sum };
        var outStream = new[] { 3, 2, 1 }.AsAsyncEnumerable();

        return (outStream, output);
    }

    [TrackingServerFilter(4, "method")]
    public async IAsyncEnumerable<string> DuplexStreamSync(IAsyncEnumerable<string> stream, IList<string> input)
    {
        await foreach (var i in stream.ConfigureAwait(false))
        {
        }

        foreach (var i in input)
        {
            yield return i;
        }

        yield return "implementation";
    }
}