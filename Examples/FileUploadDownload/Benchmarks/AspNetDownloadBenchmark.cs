using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class AspNetDownloadBenchmark : AspNetBenchmarkBase
{
    [Benchmark]
    public Task Default()
    {
        return CallsDefault.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark]
    public Task RentedArray()
    {
        return CallsRentedArray.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public Task HttpClient()
    {
        return CallsHttpClient.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }
}