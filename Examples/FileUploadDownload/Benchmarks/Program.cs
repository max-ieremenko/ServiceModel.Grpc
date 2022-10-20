using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client.Web;

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
        await RunAspNetDownloadBenchmark();
        await RunAspNetUploadBenchmark();

        await RunGrpcWebDownloadBenchmark();

        await RunSelfHostDownloadBenchmark();
        await RunSelfHostUploadBenchmark();

        await RunCoreVsNetChannelDownloadBenchmark();
        await RunCoreVsNetChannelUploadBenchmark();
    }
#endif

    private static async Task RunAspNetDownloadBenchmark()
    {
        var benchmark = new AspNetDownloadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.Default();
            await benchmark.RentedArray();
            await benchmark.HttpClient();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunAspNetUploadBenchmark()
    {
        var benchmark = new AspNetUploadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.Default();
            await benchmark.RentedArray();
            await benchmark.HttpClient();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunGrpcWebDownloadBenchmark()
    {
        var benchmark = new GrpcWebDownloadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        foreach (var mode in new[] { GrpcWebMode.GrpcWeb, GrpcWebMode.GrpcWebText })
        {
            benchmark.UseCompression = useCompression;
            benchmark.Mode = mode;
            await benchmark.GlobalSetup();

            await benchmark.Default();
            await benchmark.RentedArray();
            await benchmark.HttpClient();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunSelfHostDownloadBenchmark()
    {
        var benchmark = new SelfHostDownloadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            benchmark.GlobalSetup();

            await benchmark.Default();
            await benchmark.RentedArray();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunSelfHostUploadBenchmark()
    {
        var benchmark = new SelfHostUploadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            benchmark.GlobalSetup();

            await benchmark.Default();
            await benchmark.RentedArray();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunCoreVsNetChannelDownloadBenchmark()
    {
        var benchmark = new CoreVsNetChannelDownloadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.CoreChannel();
            await benchmark.NetChannel();
            await benchmark.HttpClient();

            await benchmark.GlobalCleanup();
        }
    }

    private static async Task RunCoreVsNetChannelUploadBenchmark()
    {
        var benchmark = new CoreVsNetChannelUploadBenchmark
        {
            BufferSize = StreamExtensions.StreamDefaultCopyBufferSize
        };

        foreach (var useCompression in new[] { true, false })
        {
            benchmark.UseCompression = useCompression;
            await benchmark.GlobalSetup();

            await benchmark.CoreChannel();
            await benchmark.NetChannel();
            await benchmark.HttpClient();

            await benchmark.GlobalCleanup();
        }
    }

}