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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Server;

internal sealed class ProtobufGrpcServerCallTest : IUnaryCallTest
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly StubHttpRequest _request;

    public ProtobufGrpcServerCallTest(SomeObject payload)
    {
        _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        _client = _server.CreateClient();

        _request = new StubHttpRequest(
            _client,
            "/ServiceModel.Grpc.Benchmarks.Domain.TestService/PingPong",
            MessageSerializer.Create(ProtobufMarshallerFactory.Default, payload));
    }

    public Task StartAsync() => Task.CompletedTask;

    public Task PingPongAsync() => _request.SendAsync();

    public async ValueTask<long> GetPingPongPayloadSize()
    {
        await PingPongAsync().ConfigureAwait(false);
        return _request.PayloadSize;
    }

    public ValueTask DisposeAsync()
    {
        _server.Dispose();
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    private sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCodeFirstGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TestServiceStub>();
            });
        }
    }
}