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

using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Benchmarks.UnaryCallTest;
using ServiceModel.Grpc.Benchmarks.UnaryCallTest.Client;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks;

public class ClientUnaryCallBenchmark : UnaryCallBenchmarkBase
{
    internal override IUnaryCallTest CreateServiceModelGrpc(IMarshallerFactory marshallerFactory, SomeObject payload) =>
        new ServiceModelGrpcClientCallTest(marshallerFactory, payload);

    internal override IUnaryCallTest CreateServiceModelGrpcProto(SomeObjectProto payload) =>
        new ServiceModelGrpcProtoClientCallTest(payload);

    internal override IUnaryCallTest CreateNativeGrpc(SomeObjectProto payload) =>
        new NativeGrpcClientCallTest(payload);

    internal override IUnaryCallTest CreateProtobufGrpc(SomeObject payload) =>
        new ProtobufGrpcClientCallTest(payload);

    internal override IUnaryCallTest CreateMagicOnion(SomeObject payload) =>
        new MagicOnionClientCallTest(payload);
}