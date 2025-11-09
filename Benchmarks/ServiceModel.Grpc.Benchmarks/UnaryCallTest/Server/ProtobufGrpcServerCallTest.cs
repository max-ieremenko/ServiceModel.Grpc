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

using Microsoft.AspNetCore.Builder;
using ProtoBuf.Grpc.Server;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Server;

internal sealed class ProtobufGrpcServerCallTest : IUnaryCallTest
{
    private readonly byte[] _payload;
    private readonly TestServerHost _host;
    private StubHttpRequest _request = null!;

    public ProtobufGrpcServerCallTest(SomeObject payload)
    {
        _payload = MessageSerializer.Create(ProtobufMarshallerFactory.Default, payload);

        _host = new TestServerHost()
            .ConfigureServices(services =>
            {
                services.AddCodeFirstGrpc();
            })
            .ConfigureEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TestServiceStub>();
            });
    }

    public async Task StartAsync()
    {
        await _host.StartAsync().ConfigureAwait(false);
        _request = new StubHttpRequest(_host.GetClient(), "/ServiceModel.Grpc.Benchmarks.Domain.TestService/PingPong", _payload);
    }

    public Task PingPongAsync() => _request.SendAsync();

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}