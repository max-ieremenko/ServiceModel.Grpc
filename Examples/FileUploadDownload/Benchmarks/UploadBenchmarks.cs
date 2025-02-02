using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class UploadBenchmarks : BenchmarkBase
{
    [Benchmark]
    public Task Grpc() => GrpcClient.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);

    [Benchmark(Baseline = true)]
    public Task Http() => HttpClient.UploadFileAsync(FilePath, BufferSize, CancellationToken.None);
}