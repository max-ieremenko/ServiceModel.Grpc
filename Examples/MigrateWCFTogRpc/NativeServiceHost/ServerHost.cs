using Contract;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NativeServiceHost;

internal sealed class ServerHost : IHostedService
{
    private readonly Server _server;

    public ServerHost(IServiceProvider serviceProvider)
    {
        _server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", SharedConfiguration.NativegRPCPersonServicePort, ServerCredentials.Insecure)
            }
        };

        _server.Services.AddServiceModel<PersonService>(serviceProvider);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}