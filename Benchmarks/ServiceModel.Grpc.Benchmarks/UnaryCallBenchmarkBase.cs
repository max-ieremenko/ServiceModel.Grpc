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
using ServiceModel.Grpc.Benchmarks.Api;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Benchmarks.UnaryCallTest;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public abstract class UnaryCallBenchmarkBase
{
    private IUnaryCallTest _serviceModelGrpcDataContract = null!;
    private IUnaryCallTest _serviceModelGrpcProtobuf = null!;
    private IUnaryCallTest _serviceModelGrpcMessagePack = null!;
    private IUnaryCallTest _serviceModelGrpcProto = null!;
    private IUnaryCallTest _native = null!;
    private IUnaryCallTest _protobufGrpc = null!;
    private IUnaryCallTest _magicOnion = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var payload = DomainExtensions.CreateSomeObject();
        var protoPayload = DomainExtensions.CopyToProto(payload);
        _serviceModelGrpcDataContract = CreateServiceModelGrpc(DataContractMarshallerFactory.Default, payload);
        _serviceModelGrpcProtobuf = CreateServiceModelGrpc(ProtobufMarshallerFactory.Default, payload);
        _serviceModelGrpcMessagePack = CreateServiceModelGrpc(MessagePackMarshallerFactory.Default, payload);
        _serviceModelGrpcProto = CreateServiceModelGrpcProto(protoPayload);
        _native = CreateNativeGrpc(protoPayload);
        _protobufGrpc = CreateProtobufGrpc(payload);
        _magicOnion = CreateMagicOnion(payload);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _serviceModelGrpcDataContract?.Dispose();
        _serviceModelGrpcProtobuf?.Dispose();
        _serviceModelGrpcMessagePack?.Dispose();
        _serviceModelGrpcProto?.Dispose();
        _native?.Dispose();
        _protobufGrpc?.Dispose();
        _magicOnion?.Dispose();
    }

    [Benchmark(Description = "ServiceModelGrpc.DataContract")]
    [PayloadSizeColumn(nameof(GetServiceModelGrpcDataContractSize))]
    public Task ServiceModelGrpcDataContract() => _serviceModelGrpcDataContract.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.Protobuf")]
    [PayloadSizeColumn(nameof(GetServiceModelGrpcProtobufSize))]
    public Task ServiceModelGrpcProtobuf() => _serviceModelGrpcProtobuf.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.MessagePack")]
    [PayloadSizeColumn(nameof(GetServiceModelGrpcMessagePackSize))]
    public Task ServiceModelGrpcMessagePack() => _serviceModelGrpcMessagePack.PingPongAsync();

    [Benchmark(Baseline = true, Description = "grpc-dotnet")]
    [PayloadSizeColumn(nameof(GetNativeSize))]
    public Task Native() => _native.PingPongAsync();

    [Benchmark(Description = "ServiceModelGrpc.proto-emulation")]
    [PayloadSizeColumn(nameof(GetServiceModelGrpcProtoSize))]
    public Task ServiceModelGrpcProto() => _serviceModelGrpcProto.PingPongAsync();

    [Benchmark(Description = "protobuf-net.Grpc")]
    [PayloadSizeColumn(nameof(GetProtobufGrpcSize))]
    public Task ProtobufGrpc() => _protobufGrpc.PingPongAsync();

    [Benchmark]
    [PayloadSizeColumn(nameof(GetMagicOnionSize))]
    public Task MagicOnion() => _magicOnion.PingPongAsync();

    internal ValueTask<long> GetServiceModelGrpcDataContractSize()
    {
        return GetSize(() => _serviceModelGrpcDataContract);
    }

    internal ValueTask<long> GetServiceModelGrpcProtobufSize()
    {
        return GetSize(() => _serviceModelGrpcProtobuf);
    }

    internal ValueTask<long> GetServiceModelGrpcMessagePackSize()
    {
        return GetSize(() => _serviceModelGrpcMessagePack);
    }

    internal ValueTask<long> GetServiceModelGrpcProtoSize()
    {
        return GetSize(() => _serviceModelGrpcProto);
    }

    internal ValueTask<long> GetNativeSize()
    {
        return GetSize(() => _native);
    }

    internal ValueTask<long> GetProtobufGrpcSize()
    {
        return GetSize(() => _protobufGrpc);
    }

    internal ValueTask<long> GetMagicOnionSize()
    {
        return GetSize(() => _magicOnion);
    }

    internal abstract IUnaryCallTest CreateServiceModelGrpc(IMarshallerFactory marshallerFactory, SomeObject payload);

    internal abstract IUnaryCallTest CreateServiceModelGrpcProto(SomeObjectProto payload);

    internal abstract IUnaryCallTest CreateNativeGrpc(SomeObjectProto payload);

    internal abstract IUnaryCallTest CreateProtobufGrpc(SomeObject payload);

    internal abstract IUnaryCallTest CreateMagicOnion(SomeObject payload);

    private async ValueTask<long> GetSize(Func<IUnaryCallTest> sut)
    {
        GlobalSetup();
        var result = await sut().GetPingPongPayloadSize().ConfigureAwait(false);
        GlobalCleanup();
        return result;
    }
}