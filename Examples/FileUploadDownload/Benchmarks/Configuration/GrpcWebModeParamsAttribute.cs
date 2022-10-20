using BenchmarkDotNet.Attributes;
using Grpc.Net.Client.Web;

namespace Benchmarks.Configuration;

public class GrpcWebModeParamsAttribute : ParamsAttribute
{
    public GrpcWebModeParamsAttribute()
        : base(GrpcWebMode.GrpcWeb, GrpcWebMode.GrpcWebText)
    {
    }
}