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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceModel.Grpc.Client.DependencyInjection.Internal;

internal sealed class ClientFactoryResolver
{
    private const string LoggerName = "ServiceModel.Grpc.Client";

    public List<IClientResolver> Clients { get; } = new();

    public IChannelProvider? Channel { get; set; }

    public IClientFactory Resolve(IServiceProvider provider) => Build(provider, null);

    internal IClientFactory Build(IServiceProvider provider, Func<ServiceModelGrpcClientOptions, IClientFactory>? test)
    {
        var options = provider.GetService<IOptions<ServiceModelGrpcClientOptions>>()?.Value ?? new ServiceModelGrpcClientOptions();

        if (options.ServiceProvider == null)
        {
            options.ServiceProvider = provider;
        }

        if (options.Logger == null)
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(LoggerName);
            options.Logger = LogAdapter.Wrap(logger);
        }

        var result = test == null ? new ClientFactory(options) : test(options);
        for (var i = 0; i < Clients.Count; i++)
        {
            var client = Clients[i];
            if (client.Channel == null)
            {
                client.Channel = Channel;
            }

            client.Setup(provider, result);
        }

        return result;
    }
}