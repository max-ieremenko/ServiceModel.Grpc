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
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.SelfHost
{
    internal sealed class ServerHost : IAsyncDisposable
    {
        private const int Port = 8080;
        private readonly Server _server;

        public ServerHost()
        {
            _server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", Port, ServerCredentials.Insecure)
                }
            };

            Channel = new GrpcChannel("localhost", Port, ChannelCredentials.Insecure);
        }

        public Server.ServiceDefinitionCollection Services => _server.Services;

        public GrpcChannel Channel { get; }

        public void Start()
        {
            _server.Start();
        }

        public async ValueTask DisposeAsync()
        {
            await Channel.ShutdownAsync();
            await _server.ShutdownAsync();
        }
    }
}
