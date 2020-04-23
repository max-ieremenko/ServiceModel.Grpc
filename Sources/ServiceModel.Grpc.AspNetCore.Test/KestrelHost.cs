// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.Configuration;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.AspNetCore
{
    internal sealed partial class KestrelHost : IAsyncDisposable
    {
        private readonly int _port;
        private IHost _host;

        public KestrelHost(
            IMarshallerFactory marshallerFactory = null,
            int port = 8080,
            CallOptions? defaultCallOptions = null)
        {
            _marshallerFactory = marshallerFactory;
            _port = port;

            var factoryOptions = new ServiceModelGrpcClientOptions { MarshallerFactory = _marshallerFactory };
            if (defaultCallOptions.HasValue)
            {
                factoryOptions.DefaultCallOptionsFactory = () => defaultCallOptions.Value;
            }

            ClientFactory = new ClientFactory(factoryOptions);
        }

        public GrpcChannel Channel { get; private set; }

        public IClientFactory ClientFactory { get; }

        public async Task StartAsync(
            Action<IServiceCollection> configureServices = null,
            Action<IApplicationBuilder> configureApp = null,
            Action<IEndpointRouteBuilder> configureEndpoints = null)
        {
            GrpcChannelExtensions.Http2UnencryptedSupport = true;
            _configureServices = configureServices;
            _configureApp = configureApp;
            _configureEndpoints = configureEndpoints;

            _host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => o.ListenLocalhost(_port, l => l.Protocols = HttpProtocols.Http2));
                })
                .Build();

            Channel = new GrpcChannel("localhost", _port, ChannelCredentials.Insecure);
            await _host.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            _marshallerFactory = null;
            _configureServices = null;
            _configureApp = null;
            _configureEndpoints = null;

            if (Channel != null)
            {
                await Channel.ShutdownAsync();
            }

            if (_host != null)
            {
                await _host.StopAsync();
            }
        }
    }
}
