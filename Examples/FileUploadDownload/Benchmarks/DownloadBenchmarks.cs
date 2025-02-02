using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class DownloadBenchmarks : BenchmarkBase
{
    [Benchmark]
    public Task Grpc() => GrpcClient.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);

    [Benchmark(Baseline = true)]
    public Task Http() => HttpClient.DownloadFileAsync(FilePath, BufferSize, CancellationToken.None);
}