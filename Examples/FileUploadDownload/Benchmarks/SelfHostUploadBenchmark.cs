using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class SelfHostUploadBenchmark : SelfHostBenchmarkBase
    {
        [Benchmark]
        public Task Default()
        {
            return CallsDefault.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }

        [Benchmark(Baseline = true)]
        public Task RentedArray()
        {
            return CallsRentedArray.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }
    }
}