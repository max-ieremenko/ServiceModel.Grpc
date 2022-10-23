using BenchmarkDotNet.Attributes;

namespace Benchmarks.Configuration;

public class CompressionParamsAttribute : ParamsAttribute
{
    public CompressionParamsAttribute()
        : base(true, false)
    {
    }
}