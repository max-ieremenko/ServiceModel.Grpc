using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Service;
using ServiceModel.Grpc.Interceptors;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core.Logging;

namespace ServerNativeHost;

internal sealed class ServerHost : IHostedService
{
    private readonly Server _server;
    
    public ServerHost()
    {
        _server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", ServiceConfiguration.ServiceNativeGrpcPort, ServerCredentials.Insecure)
            }
        };

        _server.Services.AddServiceModelSingleton(
            new DebugService(),
            options =>
            {
                // enable ServiceModel.Grpc logging
                options.Logger = new ConsoleLogger();

                // combine application and unexpected handlers into one handler
                options.ErrorHandler = new ServerErrorHandlerCollection(
                    new ApplicationExceptionServerHandler(),
                    new UnexpectedExceptionServerHandler());
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}