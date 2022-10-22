using System;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace ServerSelfHost;

internal class ServerHost : IHostedService
{
    private readonly Server _server;

    public ServerHost(IServiceProvider serviceProvider)
    {
        _server = new Server
        {
            Ports = { new ServerPort("localhost", 5000, ServerCredentials.Insecure) }
        };

        // host GreeterService
        _server.Services.AddServiceModel<GreeterService>(
            serviceProvider,
            options =>
            {
                options.ConfigureServiceDefinition = definition =>
                {
                    var interceptor = options.ServiceProvider!.GetRequiredService<ServerLoggerInterceptor>();
                    return definition.Intercept(interceptor);
                };
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}