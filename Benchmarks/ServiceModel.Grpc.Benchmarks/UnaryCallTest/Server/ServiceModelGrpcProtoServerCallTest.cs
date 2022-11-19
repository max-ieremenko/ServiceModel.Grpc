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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Benchmarks.Api;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Server;

internal sealed class ServiceModelGrpcProtoServerCallTest : IUnaryCallTest
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly StubHttpRequest _request;

    public ServiceModelGrpcProtoServerCallTest(SomeObject payload)
    {
        var builder = new WebHostBuilder().UseStartup<Startup>();

        _server = new TestServer(builder);
        _client = _server.CreateClient();

        var proto = DomainExtensions.CopyToProto(payload);
        _request = new StubHttpRequest(
            _client,
            "/ITestService/PingPongProto",
            MessageSerializer.Create(GoogleProtoMarshallerFactory.Default, new Message<SomeObjectProto>(proto)));
    }

    public Task PingPongAsync() => _request.SendAsync();

    public async ValueTask<long> GetPingPongPayloadSize()
    {
        await PingPongAsync().ConfigureAwait(false);
        return _request.PayloadSize;
    }

    public void Dispose()
    {
        _server.Dispose();
        _client.Dispose();
    }

    private sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelGrpc(options => options.DefaultMarshallerFactory = GoogleProtoMarshallerFactory.Default);
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