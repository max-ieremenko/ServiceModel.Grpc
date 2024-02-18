using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Benchmarks.Configuration;
using ConsoleClient;
using ConsoleClient.Internal;
using Contract;
using Microsoft.Extensions.Hosting;

namespace Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class CoreVsNetChannelBenchmarkBase
{
    private IHost _serverAspNetHost = null!;

    [BufferSizeParams]
    public int BufferSize { get; set; }

    [CompressionParams]
    public bool UseCompression { get; set; }

    protected IClientCalls CallsCoreChannel { get; private set; } = null!;

    protected IClientCalls CallsNetChannel { get; private set; } = null!;

    protected IClientCalls CallsHttpClient { get; private set; } = null!;

    protected string FilePath { get; private set; } = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serverAspNetHost = ServerAspNetHost.Program.BuildHost(UseCompression);
        await _serverAspNetHost.StartAsync();

        CallsCoreChannel = ClientCallsFactory
            .ForChannel(Hosts.ServerAspNetHostHttp2, ChannelType.GrpcCore)
            .WithCompression(UseCompression)
            .CreateFileServiceRentedArray();

        CallsNetChannel = ClientCallsFactory
            .ForChannel(Hosts.ServerAspNetHostHttp2, ChannelType.GrpcNet)
            .WithCompression(UseCompression)
            .CreateFileServiceRentedArray();

        CallsHttpClient = ClientCallsFactory.CreateHttpClient(UseCompression);

        FilePath = ConsoleClient.Program.FindDemoFile();
        ProgressBar.Disable();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await CallsCoreChannel.DisposeAsync();
        await CallsNetChannel.DisposeAsync();
        await CallsHttpClient.DisposeAsync();

        await _serverAspNetHost.StopAsync();
        _serverAspNetHost.Dispose();
    }
}