// <copyright>
// Copyright 2021 Max Ieremenko
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

using System;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.Benchmarks;

public static class Program
{
#if RELEASE
        public static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
#else
    public static async Task Main(string[] args)
    {
        await RunUnaryCallBenchmark<CombinedUnaryCallBenchmark>();
        await RunUnaryCallBenchmark<ClientUnaryCallBenchmark>();
        await RunUnaryCallBenchmark<ServerUnaryCallBenchmark>();

        RunMarshallerBenchmark<MessagePackMarshallerBenchmark>();
        RunMarshallerBenchmark<ProtobufMarshallerBenchmark>();
    }

    private static async Task RunUnaryCallBenchmark<T>()
        where T : UnaryCallBenchmarkBase, new()
    {
        var benchmark = new T();
        Console.WriteLine("---- {0} -----", benchmark.GetType().Name);
        benchmark.GlobalSetup();

        await benchmark.ServiceModelGrpcDataContract().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.ServiceModelGrpcDataContract), await new T().GetServiceModelGrpcDataContractSize());

        await benchmark.ServiceModelGrpcProtobuf().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.ServiceModelGrpcProtobuf), await new T().GetServiceModelGrpcProtobufSize());

        await benchmark.ServiceModelGrpcMessagePack().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.ServiceModelGrpcMessagePack), await new T().GetServiceModelGrpcMessagePackSize());

        await benchmark.ServiceModelGrpcProto().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.ServiceModelGrpcProto), await new T().GetServiceModelGrpcProtoSize());

        await benchmark.Native().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.Native), await new T().GetNativeSize());

        await benchmark.ProtobufGrpc().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.ProtobufGrpc), await new T().GetProtobufGrpcSize());

        await benchmark.MagicOnion().ConfigureAwait(false);
        Console.WriteLine("{0}: {1}", nameof(benchmark.MagicOnion), await new T().GetMagicOnionSize());

        benchmark.GlobalCleanup();
    }

    private static void RunMarshallerBenchmark<T>()
        where T : MarshallerBenchmarkBase, new()
    {
        var benchmark = new T();
        Console.WriteLine("---- {0} -----", benchmark.GetType().Name);
        benchmark.GlobalSetup();

        benchmark.DefaultSerializer();
        benchmark.DefaultDeserializer();

        benchmark.StreamSerializer();
        benchmark.StreamDeserializer();
    }
#endif
}