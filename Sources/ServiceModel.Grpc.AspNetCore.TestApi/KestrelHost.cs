// <copyright>
// Copyright 2020-2023 Max Ieremenko
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

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Client.DependencyInjection;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.AspNetCore.TestApi;

public sealed class KestrelHost : IAsyncDisposable
{
    private GrpcChannelType _channelType;
    private IWebHost? _host;
    private Action<ServiceModelGrpcClientOptions>? _clientFactoryDefaultOptions;
    private Action<IServiceCollection>? _configureServices;
    private Action<IEndpointRouteBuilder>? _configureEndpoints;
    private Action<IApplicationBuilder>? _configureApp;

    public KestrelHost()
    {
        _channelType = GrpcChannelType.GrpcCore;
    }

    public ChannelBase Channel => Services.GetRequiredService<ChannelBase>();

    public IClientFactory ClientFactory => Services.GetRequiredService<IClientFactory>();

    public IServiceProvider Services => _host!.Services;

    public KestrelHost ConfigureClientFactory(Action<ServiceModelGrpcClientOptions> configuration)
    {
        _clientFactoryDefaultOptions += configuration;
        return this;
    }

    public KestrelHost ConfigureApp(Action<IApplicationBuilder> configuration)
    {
        _configureApp += configuration;
        return this;
    }

    public KestrelHost ConfigureServices(Action<IServiceCollection> configuration)
    {
        _configureServices += configuration;
        return this;
    }

    public KestrelHost ConfigureEndpoints(Action<IEndpointRouteBuilder> configuration)
    {
        _configureEndpoints += configuration;
        return this;
    }

    public KestrelHost WithChannelType(GrpcChannelType channelType)
    {
        _channelType = channelType;
        return this;
    }

    public string GetLocation(string? relativePath = default)
    {
        var root = "http://" + Channel.Target;
        if (string.IsNullOrEmpty(relativePath))
        {
            return root;
        }

        return new Uri(new Uri(root), relativePath).ToString();
    }

    public async Task<KestrelHost> StartAsync(HttpProtocols protocols = HttpProtocols.Http2)
    {
        GrpcChannelExtensions.Http2UnencryptedSupport = true;

        _host = WebHost
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddGrpc();
                services.AddServiceModelGrpc((options, provider) =>
                {
                    var clientOptions = provider.GetRequiredService<IOptions<ServiceModelGrpcClientOptions>>().Value;
                    options.DefaultMarshallerFactory = clientOptions.MarshallerFactory;
                });

                services.AddSingleton<ChannelBase>(provider =>
                {
                    var address = provider
                        .GetRequiredService<IServer>()
                        .Features
                        .Get<IServerAddressesFeature>()
                        !.Addresses
                        .First();

                    return GrpcChannelFactory.CreateChannel(_channelType, "localhost", new Uri(address).Port);
                });

                services
                    .AddServiceModelGrpcClientFactory((options, _) =>
                    {
                        _clientFactoryDefaultOptions?.Invoke(options);
                    });

                _configureServices?.Invoke(services);
            })
            .Configure(app =>
            {
                app.UseRouting();

                _configureApp?.Invoke(app);

                if (_configureEndpoints != null)
                {
                    app.UseEndpoints(_configureEndpoints);
                }
            })
            .UseKestrel(o => o.Listen(IPAddress.Loopback, 0, l => l.Protocols = protocols))
            .ConfigureLogging(builder => SuppressLogging(builder))
            .Build();

        try
        {
            await _host.StartAsync().ConfigureAwait(false);
        }
        catch
        {
            await DisposeAsync().ConfigureAwait(false);
            throw;
        }

        return this;
    }

    public async ValueTask DisposeAsync()
    {
        _configureApp = null;
        _configureServices = null;
        _configureEndpoints = null;

        await Channel.ShutdownAsync().ConfigureAwait(false);

        if (_host != null)
        {
            try
            {
                await _host.StopAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            _host.Dispose();
        }
    }

    [Conditional("RELEASE")]
    private static void SuppressLogging(ILoggingBuilder builder)
    {
        builder.ClearProviders();
    }
}