using System.Threading.Tasks;
using Client;
using Grpc.Core;
using Service;

namespace Demo.SelfHost.ReflectionEmit;

public static class Program
{
    private const int Port = 8082;

    public static async Task Main()
    {
        var server = new Server
        {
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        // host PersonService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        server.Services.AddServiceModelTransient(() => new PersonService());

        server.Start();

        try
        {
            // a proxy for IPersonService will be generated at runtime by ServiceModel.Grpc
            var clientCalls = new ClientCalls();
            await clientCalls.CallPersonService(Port);
        }
        finally
        {
            await server.ShutdownAsync();
        }
    }
}