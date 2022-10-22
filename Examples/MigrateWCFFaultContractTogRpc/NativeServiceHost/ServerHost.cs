using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;
using ServiceModel.Grpc.Interceptors;

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
                new ServerPort("localhost", SharedConfiguration.NativegRPCDebugServicePort, ServerCredentials.Insecure)
            }
        };

        _server.Services.AddServiceModel<DebugService>(
            serviceProvider,
            options =>
            {
                // register server error handler
                options.ErrorHandler = options.ServiceProvider!.GetRequiredService<IServerErrorHandler>();
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}