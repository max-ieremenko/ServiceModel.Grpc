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

using Grpc.Core;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.SelfHost;

internal sealed class ServerHost : IAsyncDisposable
{
    private readonly GrpcChannelType _channelType;
    private readonly Server _server;

    public ServerHost(GrpcChannelType channelType = GrpcChannelType.GrpcCore)
    {
        _channelType = channelType;
        _server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", 0, ServerCredentials.Insecure)
            }
        };
    }

    public Server.ServiceDefinitionCollection Services => _server.Services;

    public ChannelBase Channel { get; private set; } = null!;

    public void Start()
    {
        _server.Start();

        var port = _server.Ports.First().BoundPort;
        Channel = GrpcChannelFactory.CreateChannel(_channelType, "localhost", port);
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.ShutdownAsync().ConfigureAwait(false);
        await _server.ShutdownAsync().ConfigureAwait(false);
    }
}