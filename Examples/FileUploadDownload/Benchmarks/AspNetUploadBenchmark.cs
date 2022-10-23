using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class AspNetUploadBenchmark : AspNetBenchmarkBase
{
    [Benchmark]
    public Task Default()
    {
        return CallsDefault.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark]
    public Task RentedArray()
    {
        return CallsRentedArray.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public Task HttpClient()
    {
        return CallsHttpClient.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
    }
}