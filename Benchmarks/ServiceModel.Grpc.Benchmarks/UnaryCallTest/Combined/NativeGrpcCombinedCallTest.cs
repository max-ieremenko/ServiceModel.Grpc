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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Benchmarks.Domain;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined;

internal sealed class NativeGrpcCombinedCallTest : IUnaryCallTest
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly GrpcChannel _channel;
    private readonly SomeObjectProto _payload;
    private readonly TestServiceNative.TestServiceNativeClient _proxy;

    public NativeGrpcCombinedCallTest(SomeObjectProto payload)
    {
        _payload = payload;
        _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        _client = _server.CreateClient();

        _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = _client });

        _proxy = new TestServiceNative.TestServiceNativeClient(_channel);
    }

    public Task StartAsync() => Task.CompletedTask;

    public async Task PingPongAsync()
    {
        using (var call = _proxy.PingPongAsync(_payload))
        {
            await call.ResponseAsync.ConfigureAwait(false);
        }
    }

    public ValueTask<long> GetPingPongPayloadSize()
    {
        return StubHttpMessageHandler.GetPayloadSize(channel =>
        {
            var proxy = new TestServiceNative.TestServiceNativeClient(channel);
            return proxy.PingPongAsync(_payload).ResponseAsync;
        });
    }

    public ValueTask DisposeAsync()
    {
        _channel.Dispose();
        _client.Dispose();
        _server.Dispose();
        return ValueTask.CompletedTask;
    }

    private sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TestServiceNativeStub>();
            });
        }
    }
}