using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Microsoft.Extensions.Hosting;

namespace ServerNativeHost;

internal class ServerHost : IHostedService
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

        _server.Services.Add(CalculatorNative.BindService(new CalculatorService()));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}