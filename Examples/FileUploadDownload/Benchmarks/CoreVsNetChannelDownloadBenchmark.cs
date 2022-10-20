using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class CoreVsNetChannelDownloadBenchmark : CoreVsNetChannelBenchmarkBase
{
    [Benchmark]
    public Task CoreChannel()
    {
        return CallsCoreChannel.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark]
    public Task NetChannel()
    {
        return CallsNetChannel.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public Task HttpClient()
    {
        return CallsHttpClient.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }
}