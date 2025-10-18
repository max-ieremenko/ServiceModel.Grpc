// <copyright>
// Copyright Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace ServiceModel.Grpc.Benchmarks;

public static class Program
{
    public static Task Main(string[] args)
    {
        if (IsRelease())
        {
            BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            return Task.CompletedTask;
        }

        return RunTests();
    }

    private static async Task RunTests()
    {
        await RunUnaryCallBenchmark<CombinedUnaryCallBenchmark>().ConfigureAwait(false);
        await RunUnaryCallBenchmark<ClientUnaryCallBenchmark>().ConfigureAwait(false);
        await RunUnaryCallBenchmark<ServerUnaryCallBenchmark>().ConfigureAwait(false);

        RunMarshallerBenchmark<MessagePackMarshallerBenchmark>();
        RunMarshallerBenchmark<MemoryPackMarshallerBenchmark>();
        RunMarshallerBenchmark<ProtobufMarshallerBenchmark>();
    }

    private static async Task RunUnaryCallBenchmark<T>()
        where T : UnaryCallBenchmarkBase, new()
    {
        var benchmark = new T();
        Console.WriteLine("---- {0} -----", benchmark.GetType().Name);
        await benchmark.GlobalSetup().ConfigureAwait(false);

        await benchmark.ServiceModelGrpcDataContract().ConfigureAwait(false);

        await benchmark.ServiceModelGrpcProtobuf().ConfigureAwait(false);

        await benchmark.ServiceModelGrpcMessagePack().ConfigureAwait(false);

        await benchmark.ServiceModelGrpcMemoryPack().ConfigureAwait(false);

        await benchmark.ServiceModelGrpcProto().ConfigureAwait(false);

        await benchmark.Native().ConfigureAwait(false);

        await benchmark.ProtobufGrpc().ConfigureAwait(false);

        await benchmark.MagicOnion().ConfigureAwait(false);

        await benchmark.GlobalCleanup().ConfigureAwait(false);
    }

    private static void RunMarshallerBenchmark<T>()
        where T : MarshallerBenchmarkBase, new()
    {
        var benchmark = new T();
        Console.WriteLine("---- {0} -----", benchmark.GetType().Name);
        benchmark.GlobalSetup();

        benchmark.Serialize();
        benchmark.Deserialize();
    }

    private static bool IsRelease()
    {
#if RELEASE
        return true;
#else
        return false;
#endif
    }
}