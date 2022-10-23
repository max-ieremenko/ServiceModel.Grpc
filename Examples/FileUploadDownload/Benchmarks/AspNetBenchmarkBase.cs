using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Benchmarks.Configuration;
using ConsoleClient;
using ConsoleClient.Internal;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.Hosting;

namespace Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class AspNetBenchmarkBase
{
    private IHost _serverAspNetHost;

    [BufferSizeParams]
    public int BufferSize { get; set; }

    [CompressionParams]
    public bool UseCompression { get; set; }

    internal GrpcWebMode? ModeInternal { get; set; }

    protected IClientCalls CallsDefault { get; private set; }

    protected IClientCalls CallsRentedArray { get; private set; }

    protected IClientCalls CallsHttpClient { get; private set; }

    protected string FilePath { get; private set; }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serverAspNetHost = ServerAspNetHost.Program.BuildHost(UseCompression);
        await _serverAspNetHost.StartAsync();

        var factory = ModeInternal.HasValue ? ClientCallsFactory.ForAspNetHost(ModeInternal.Value) : ClientCallsFactory.ForAspNetHost(ChannelType.GrpcNet);
        factory.WithCompression(UseCompression);

        CallsDefault = factory.CreateFileService();
        CallsRentedArray = factory.CreateFileServiceRentedArray();

        CallsHttpClient = ClientCallsFactory.CreateHttpClient(UseCompression);

        FilePath = ConsoleClient.Program.FindDemoFile();
        ProgressBar.Disable();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await CallsDefault.DisposeAsync();
        await CallsRentedArray.DisposeAsync();
        await CallsHttpClient.DisposeAsync();

        await _serverAspNetHost.StopAsync();
        _serverAspNetHost.Dispose();
    }
}