using System.Linq;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;

namespace ServerNativeHost
{
    internal sealed class CalculatorService : CalculatorNative.CalculatorNativeBase
    {
        public override Task<SumResponse> Sum(SumRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SumResponse
            {
                Result = request.X + request.Y + request.Z
            });
        }

        public override async Task<SumResponse> SumValues(IAsyncStreamReader<Int32Value> requestStream, ServerCallContext context)
        {
            var result = 0L;

            while (await requestStream.MoveNext())
            {
                result += requestStream.Current.Value;
            }

            return new SumResponse { Result = result };
        }

        public override async Task Range(RangeRequest request, IServerStreamWriter<Int32Value> responseStream, ServerCallContext context)
        {
            foreach (var i in Enumerable.Range(request.Start, request.Count))
            {
                await responseStream.WriteAsync(new Int32Value { Value = i });
            }
        }

        public override async Task MultiplyBy2(IAsyncStreamReader<Int32Value> requestStream, IServerStreamWriter<Int32Value> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var value = requestStream.Current.Value * 2;
                await responseStream.WriteAsync(new Int32Value { Value = value });
            }
        }
    }
}
