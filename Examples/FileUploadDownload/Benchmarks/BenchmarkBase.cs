using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Benchmarks.Configuration;
using Client;
using Client.Internal;
using Microsoft.Extensions.Hosting;

namespace Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class BenchmarkBase
{
    private IHost _serverAspNetHost = null!;

    [BufferSizeParams]
    public int BufferSize { get; set; }

    [CompressionParams]
    public bool UseCompression { get; set; }

    protected GrpcClientCalls GrpcClient { get; private set; } = null!;

    protected HttpClientCalls HttpClient { get; private set; } = null!;

    protected string FilePath { get; private set; } = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serverAspNetHost = Server.Program.BuildHost(UseCompression);
        await _serverAspNetHost.StartAsync();

        GrpcClient = ClientCallsFactory.CreateGrpcClient(UseCompression);
        HttpClient = ClientCallsFactory.CreateHttpClient(UseCompression);

        FilePath = Client.Program.FindDemoFile();
        ProgressBar.Disable();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _serverAspNetHost.StopAsync();
        _serverAspNetHost.Dispose();
    }
}