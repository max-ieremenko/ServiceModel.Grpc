// <copyright>
// Copyright 2021-2023 Max Ieremenko
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

using System.Threading.Tasks;
using Grpc.Net.Client;
using ServiceModel.Grpc.Benchmarks.Api;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Client;

internal sealed class ServiceModelGrpcProtoClientCallTest : IUnaryCallTest
{
    private readonly SomeObjectProto _payload;
    private readonly StubHttpMessageHandler _httpHandler;
    private readonly GrpcChannel _channel;
    private readonly ITestService _proxy;

    public ServiceModelGrpcProtoClientCallTest(SomeObjectProto payload)
    {
        _payload = payload;
        _httpHandler = new StubHttpMessageHandler(MessageSerializer.Create(GoogleProtoMarshallerFactory.Default, new Message<SomeObjectProto>(_payload)));
        _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = _httpHandler });

        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions { MarshallerFactory = GoogleProtoMarshallerFactory.Default });
        _proxy = clientFactory.CreateClient<ITestService>(_channel);
    }

    public Task PingPongAsync()
    {
        return _proxy.PingPongProto(_payload);
    }

    public async ValueTask<long> GetPingPongPayloadSize()
    {
        await PingPongAsync().ConfigureAwait(false);
        return _httpHandler.PayloadSize;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _httpHandler.Dispose();
    }
}