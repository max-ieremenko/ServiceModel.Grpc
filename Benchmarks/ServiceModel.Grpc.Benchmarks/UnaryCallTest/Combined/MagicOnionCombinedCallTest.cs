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

using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Benchmarks.Domain;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined;

internal sealed class MagicOnionCombinedCallTest : IUnaryCallTest
{
    private readonly SomeObject _payload;
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly GrpcChannel _channel;
    private readonly ITestServiceMagicOnion _proxy;

    public MagicOnionCombinedCallTest(SomeObject payload)
    {
        _payload = payload;
        _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        _client = _server.CreateClient();

        _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = _client });
        _proxy = MagicOnion.Client.MagicOnionClient.Create<ITestServiceMagicOnion>(_channel);
    }

    public async Task PingPongAsync()
    {
        var call = _proxy.PingPong(_payload);
        await call;
        call.Dispose();
    }

    public ValueTask<long> GetPingPongPayloadSize()
    {
        return StubHttpMessageHandler.GetPayloadSize(async channel =>
        {
            var proxy = MagicOnion.Client.MagicOnionClient.Create<ITestServiceMagicOnion>(channel);
            var call = proxy.PingPong(_payload);
            await call;
        });
    }

    public void Dispose()
    {
        _channel.Dispose();
        _client.Dispose();
        _server.Dispose();
    }

    private sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddMagicOnion();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
            });
        }
    }
}