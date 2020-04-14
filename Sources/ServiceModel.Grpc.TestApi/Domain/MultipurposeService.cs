using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class MultipurposeService : IMultipurposeService
    {
        public string Concat(string value, CallContext context)
        {
            context.ServerCallContext.ShouldNotBeNull();

            return value + context.ServerCallContext.RequestHeaders.First(i => i.Key == "value").Value;
        }

        public Task<string> ConcatAsync(string value, CallContext context)
        {
            context.ServerCallContext.ShouldNotBeNull();

            var result = value + context.ServerCallContext.RequestHeaders.First(i => i.Key == "value").Value;
            return Task.FromResult(result);
        }

        public ValueTask<long> Sum5ValuesAsync(long x1, int x2, int x3, int x4, int x5, CancellationToken token)
        {
            return new ValueTask<long>(x1 + x2 + x3 + x4 + x5);
        }

        public async IAsyncEnumerable<string> RepeatValue(string value, int count, CallContext context)
        {
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(300);
                yield return value;
            }
        }

        public async Task<long> SumValues(IAsyncEnumerable<int> values, CallContext context)
        {
            var result = 0;
            await foreach (var i in values.WithCancellation(context.ServerCallContext.CancellationToken))
            {
                result += i;
            }

            return result;
        }

        public async IAsyncEnumerable<string> ConvertValues(IAsyncEnumerable<int> values, CallContext context)
        {
            await foreach (var i in values.WithCancellation(context.ServerCallContext.CancellationToken))
            {
                yield return i.ToString();
            }
        }
    }
}
