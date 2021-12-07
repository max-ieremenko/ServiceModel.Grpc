using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace Benchmarks.Configuration
{
    internal sealed class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);

            AddJob(Job
                .ShortRun
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithIterationCount(10)
                .WithStrategy(RunStrategy.Throughput)
                .WithGcForce(true)
                .WithGcServer(false)
                .WithPlatform(Platform.X64));

            AddExporter(MarkdownExporter.GitHub);

            AddDiagnoser(MemoryDiagnoser.Default);

            AddColumnProvider(DefaultColumnProviders.Instance);
        }
    }
}
