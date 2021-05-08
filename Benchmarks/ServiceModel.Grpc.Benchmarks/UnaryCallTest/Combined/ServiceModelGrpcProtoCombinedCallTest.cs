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
using ServiceModel.Grpc.Benchmarks.Api;
using ServiceModel.Grpc.Benchmarks.Domain;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest.Combined
{
    internal sealed class ServiceModelGrpcProtoCombinedCallTest : IUnaryCallTest
    {
        private readonly IClientFactory _clientFactory;
        private readonly SomeObjectProto _payload;
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly GrpcChannel _channel;
        private readonly ITestService _proxy;

        public ServiceModelGrpcProtoCombinedCallTest(SomeObject payload)
        {
            _payload = DomainExtensions.CopyToProto(payload);

            var builder = new WebHostBuilder().UseStartup<Startup>();
            _server = new TestServer(builder);
            _client = _server.CreateClient();

            _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = _client });
            _clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions { MarshallerFactory = GoogleProtoMarshallerFactory.Default });
            _proxy = _clientFactory.CreateClient<ITestService>(_channel);
        }

        public Task PingPongAsync() => _proxy.PingPongProto(_payload);

        public ValueTask<long> GetPingPongPayloadSize()
        {
            return StubHttpMessageHandler.GetPayloadSize(channel =>
            {
                var proxy = _clientFactory.CreateClient<ITestService>(channel);
                return proxy.PingPongProto(_payload);
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
}
