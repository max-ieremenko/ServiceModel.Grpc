using BenchmarkDotNet.Attributes;
using Contract;

namespace Benchmarks.Configuration;

public class BufferSizeParamsAttribute : ParamsAttribute
{
    public BufferSizeParamsAttribute()
        : base(
            StreamExtensions.HttpStreamCopyDefaultBufferSize,
            StreamExtensions.FileResultExecutorBufferSize,
            StreamExtensions.StreamDefaultCopyBufferSize)
    {
    }
}