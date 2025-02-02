using System.Threading.Tasks;
using Contract;

namespace Benchmarks;

public static class Program
{
#if RELEASE
    public static void Main(string[] args)
    {
        BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
#else
    public static async Task Main()
    {
        await RunDownloadBenchmarks();
        await RunUploadBenchmarks();
    }
#endif

    private static async Task RunDownloadBenchmarks()
    {
        var benchmark = new DownloadBenchmarks
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.Grpc();
            await benchmark.Http();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunUploadBenchmarks()
    {
        var benchmark = new UploadBenchmarks
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.Grpc();
            await benchmark.Http();

            await benchmark.GlobalCleanup();
        }
    }
}