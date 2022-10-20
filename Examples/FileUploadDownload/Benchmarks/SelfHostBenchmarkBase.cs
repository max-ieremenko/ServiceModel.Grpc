using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Benchmarks.Configuration;
using ConsoleClient;
using ConsoleClient.Internal;
using Grpc.Core;

namespace Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class SelfHostBenchmarkBase
{
    private Server _serverSelfHost;

    [BufferSizeParams]
    public int BufferSize { get; set; }

    [CompressionParams]
    public bool UseCompression { get; set; }

    protected IClientCalls CallsDefault { get; private set; }

    protected IClientCalls CallsRentedArray { get; private set; }

    protected string FilePath { get; private set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _serverSelfHost = ServerSelfHost.Program.BuildServer(UseCompression);
        _serverSelfHost.Start();

        var factory = ClientCallsFactory
            .ForSelfHost(ChannelType.GrpcCore)
            .WithCompression(UseCompression);

        CallsDefault = factory.CreateFileService();
        CallsRentedArray = factory.CreateFileServiceRentedArray();

        FilePath = ConsoleClient.Program.FindDemoFile();
        ProgressBar.Disable();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await CallsDefault.DisposeAsync();
        await CallsRentedArray.DisposeAsync();

        await _serverSelfHost.ShutdownAsync();
    }
}