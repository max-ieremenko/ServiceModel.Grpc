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
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceModel.Grpc.Benchmarks.UnaryCallTest;

internal sealed class TestServerHost : IAsyncDisposable
{
    private Action<IServiceCollection>? _services;
    private Action<IEndpointRouteBuilder>? _endpoints;
    private IHost? _app;

    public TestServerHost ConfigureServices(Action<IServiceCollection> configuration)
    {
        _services += configuration;
        return this;
    }

    public TestServerHost ConfigureEndpoints(Action<IEndpointRouteBuilder> configuration)
    {
        _endpoints += configuration;
        return this;
    }

    public Task StartAsync()
    {
        _app = new HostBuilder()
            .ConfigureServices(AddService)
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(builder => _endpoints?.Invoke(builder));
                    });
            })
            .Build();

        return _app.StartAsync();
    }

    public GrpcChannel GetGrpcChannel() => _app!.Services.GetRequiredService<GrpcChannel>();

    public HttpClient GetClient() => _app!.Services.GetRequiredService<HttpClient>();

    public ValueTask DisposeAsync() => (_app as IAsyncDisposable)?.DisposeAsync() ?? ValueTask.CompletedTask;

    private void AddService(IServiceCollection services)
    {
        _services?.Invoke(services);

        services.AddSingleton(provider =>
        {
            var server = (TestServer)provider.GetRequiredService<IServer>();
            return server.CreateClient();
        });

        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<HttpClient>();
            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = client });
        });
    }
}