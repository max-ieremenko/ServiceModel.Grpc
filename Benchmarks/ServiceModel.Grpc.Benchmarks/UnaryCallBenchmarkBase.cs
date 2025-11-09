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

using BenchmarkDotNet.Attributes;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Benchmarks.UnaryCallTest;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class UnaryCallBenchmarkBase
{
    private IUnaryCallTest[] _tests = null!;
    private IUnaryCallTest _serviceModelGrpcDataContract = null!;
    private IUnaryCallTest _serviceModelGrpcProtobuf = null!;
    private IUnaryCallTest _serviceModelGrpcMessagePack = null!;
    private IUnaryCallTest _serviceModelGrpcMemoryPack = null!;
    private IUnaryCallTest _serviceModelGrpcProto = null!;
    private IUnaryCallTest _native = null!;
    private IUnaryCallTest _protobufGrpc = null!;
    private IUnaryCallTest _magicOnion = null!;

    [GlobalSetup]
    public Task GlobalSetup()
    {
        var payload = DomainExtensions.CreateSomeObject();
        var protoPayload = DomainExtensions.CopyToProto(payload);

        _tests =
        [
            _serviceModelGrpcDataContract = CreateServiceModelGrpc(DataContractMarshallerFactory.Default, payload),
            _serviceModelGrpcProtobuf = CreateServiceModelGrpc(ProtobufMarshallerFactory.Default, payload),
            _serviceModelGrpcMessagePack = CreateServiceModelGrpc(MessagePackMarshallerFactory.Default, payload),
            _serviceModelGrpcMemoryPack = CreateServiceModelGrpc(MemoryPackMarshallerFactory.Default, payload),
            _serviceModelGrpcProto = CreateServiceModelGrpcProto(protoPayload),
            _native = CreateNativeGrpc(protoPayload),
            _protobufGrpc = CreateProtobufGrpc(payload),
            _magicOnion = CreateMagicOnion(payload),
        ];

        return Task.WhenAll(_tests.Select(i => i.StartAsync()));
    }

    [GlobalCleanup]
    public Task GlobalCleanup() => Task.WhenAll(_tests.Select(i => i.DisposeAsync().AsTask()));

    [Benchmark(Description = "ServiceModelGrpc.DataContract")]
    public Task ServiceModelGrpcDataContract() => _serviceModelGrpcDataContract.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.Protobuf")]
    public Task ServiceModelGrpcProtobuf() => _serviceModelGrpcProtobuf.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.MessagePack")]
    public Task ServiceModelGrpcMessagePack() => _serviceModelGrpcMessagePack.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.MemoryPack")]
    public Task ServiceModelGrpcMemoryPack() => _serviceModelGrpcMemoryPack.PingPongAsync();

    [Benchmark(Baseline = true, Description = "grpc-dotnet")]
    public Task Native() => _native.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.proto-emulation")]
    public Task ServiceModelGrpcProto() => _serviceModelGrpcProto.PingPongAsync();

    [Benchmark(Description = "protobuf-net.Grpc")]
    public Task ProtobufGrpc() => _protobufGrpc.PingPongAsync();

    [Benchmark]
    public Task MagicOnion() => _magicOnion.PingPongAsync();

    internal abstract IUnaryCallTest CreateServiceModelGrpc(IMarshallerFactory marshallerFactory, SomeObject payload);

    internal abstract IUnaryCallTest CreateServiceModelGrpcProto(SomeObjectProto payload);

    internal abstract IUnaryCallTest CreateNativeGrpc(SomeObjectProto payload);

    internal abstract IUnaryCallTest CreateProtobufGrpc(SomeObject payload);

    internal abstract IUnaryCallTest CreateMagicOnion(SomeObject payload);
}