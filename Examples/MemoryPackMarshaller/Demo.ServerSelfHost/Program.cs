using System.Threading.Tasks;
using Client;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace Demo.ServerSelfHost;

public static class Program
{
    private const int Port = 8083;

    public static async Task Main(string[] args)
    {
        var server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", Port, ServerCredentials.Insecure)
            }
        };

        GrpcServices.MapAllGrpcServices(
            server.Services,
            options =>
            {
                // set MemoryPackMarshaller as default Marshaller
                options.MarshallerFactory = MemoryPackMarshallerFactory.Default;
            });

        server.Start();

        try
        {
            await ClientCalls.RunAsync(Port);
        }
        finally
        {
            await server.ShutdownAsync();
        }
    }
}