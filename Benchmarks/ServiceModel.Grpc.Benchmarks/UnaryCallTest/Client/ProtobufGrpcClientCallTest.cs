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

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Client;

internal sealed class ProtobufGrpcClientCallTest : IUnaryCallTest
{
    private readonly SomeObject _payload;
    private readonly StubHttpMessageHandler _httpHandler;
    private readonly GrpcChannel _channel;
    private readonly ITestService _proxy;

    public ProtobufGrpcClientCallTest(SomeObject payload)
    {
        _payload = payload;
        _httpHandler = new StubHttpMessageHandler(MessageSerializer.Create(ProtobufMarshallerFactory.Default, _payload));
        _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = _httpHandler });

        _proxy = _channel.CreateGrpcService<ITestService>();
    }

    public Task StartAsync() => Task.CompletedTask;

    public Task PingPongAsync()
    {
        return _proxy.PingPong(_payload);
    }

    public ValueTask DisposeAsync()
    {
        _channel.Dispose();
        _httpHandler.Dispose();
        return ValueTask.CompletedTask;
    }
}