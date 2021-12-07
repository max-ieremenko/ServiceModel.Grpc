using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class CoreVsNetChannelUploadBenchmark : CoreVsNetChannelBenchmarkBase
    {
        [Benchmark]
        public Task CoreChannel()
        {
            return CallsCoreChannel.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }

        [Benchmark]
        public Task NetChannel()
        {
            return CallsNetChannel.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }

        [Benchmark(Baseline = true)]
        public Task HttpClient()
        {
            return CallsHttpClient.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
        }
    }
}