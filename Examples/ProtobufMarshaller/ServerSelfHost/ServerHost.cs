using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using ServiceModel.Grpc.Configuration;

namespace ServerSelfHost;

internal class ServerHost : IHostedService
{
    private readonly Server _server;

    public ServerHost()
    {
        _server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", ServiceConfiguration.SelfHostPort, ServerCredentials.Insecure)
            }
        };

        _server.Services.AddServiceModelSingleton(
            new PersonService(),
            options =>
            {
                // set ProtobufMarshaller as default Marshaller
                options.MarshallerFactory = ProtobufMarshallerFactory.Default;
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}