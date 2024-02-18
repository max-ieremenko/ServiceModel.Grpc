using Benchmarks.Configuration;
using Grpc.Net.Client.Web;

namespace Benchmarks;

public class GrpcWebDownloadBenchmark : AspNetDownloadBenchmark
{
    public GrpcWebDownloadBenchmark()
    {
        ModeInternal = GrpcWebMode.GrpcWeb;
    }

    [GrpcWebModeParams]
    public GrpcWebMode Mode
    {
        get => ModeInternal!.Value;
        set => ModeInternal = value;
    }
}