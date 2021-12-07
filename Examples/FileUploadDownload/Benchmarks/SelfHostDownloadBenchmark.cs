using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class SelfHostDownloadBenchmark : SelfHostBenchmarkBase
    {
        [Benchmark]
        public Task Default()
        {
            return CallsDefault.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }

        [Benchmark(Baseline = true)]
        public Task RentedArray()
        {
            return CallsRentedArray.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }
    }
}
