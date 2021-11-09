// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceModel.Grpc.Client;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.AspNetCore.TestApi
{
    public sealed class KestrelHost : IAsyncDisposable
    {
        private const int DefaultPort = 8080;

        private readonly int _port;
        private IHost? _host;
        private ServiceModelGrpcClientOptions? _clientFactoryDefaultOptions;
        private Action<IServiceCollection>? _configureServices;
        private Action<IEndpointRouteBuilder>? _configureEndpoints;
        private Action<IApplicationBuilder>? _configureApp;

        public KestrelHost(int port = DefaultPort)
        {
            _port = port;
        }

        public GrpcChannel Channel { get; private set; } = null!;

        public IClientFactory ClientFactory { get; private set; } = null!;

        public KestrelHost ConfigureClientFactory(Action<ServiceModelGrpcClientOptions> configuration)
        {
            var options = new ServiceModelGrpcClientOptions();
            configuration(options);
            _clientFactoryDefaultOptions = options;
            return this;
        }

        public KestrelHost ConfigureApp(Action<IApplicationBuilder> configuration)
        {
            if (_configureApp == null)
            {
                _configureApp = configuration;
            }
            else
            {
                _configureApp += configuration;
            }

            return this;
        }

        public KestrelHost ConfigureServices(Action<IServiceCollection> configuration)
        {
            if (_configureServices == null)
            {
                _configureServices = configuration;
            }
            else
            {
                _configureServices += configuration;
            }

            return this;
        }

        public KestrelHost ConfigureEndpoints(Action<IEndpointRouteBuilder> configuration)
        {
            if (_configureEndpoints == null)
            {
                _configureEndpoints = configuration;
            }
            else
            {
                _configureEndpoints += configuration;
            }

            return this;
        }

        public string GetLocation(string? relativePath = default)
        {
            var root = string.Format("http://localhost:{0}", DefaultPort);
            if (string.IsNullOrEmpty(relativePath))
            {
                return root;
            }

            return new Uri(new Uri(root), relativePath).ToString();
        }

        public async Task<KestrelHost> StartAsync(HttpProtocols protocols = HttpProtocols.Http2)
        {
            GrpcChannelExtensions.Http2UnencryptedSupport = true;

            _host = Host
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddGrpc();
                    services.AddServiceModelGrpc();
                    _configureServices?.Invoke(services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        _configureApp?.Invoke(app);

                        if (_configureEndpoints != null)
                        {
                            app.UseEndpoints(_configureEndpoints);
                        }
                    });

                    webBuilder.UseKestrel(o => o.ListenLocalhost(_port, l => l.Protocols = protocols));
                })
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

            ClientFactory = new ClientFactory(_clientFactoryDefaultOptions);
            Channel = new GrpcChannel("localhost", DefaultPort, ChannelCredentials.Insecure);

            return this;
        }

        public async ValueTask DisposeAsync()
        {
            _configureApp = null;
            _configureServices = null;
            _configureEndpoints = null;

            if (Channel != null)
            {
                await Channel.ShutdownAsync().ConfigureAwait(false);
            }

            if (_host != null)
            {
                await _host.StopAsync().ConfigureAwait(false);
                _host.Dispose();
            }
        }
    }
}
