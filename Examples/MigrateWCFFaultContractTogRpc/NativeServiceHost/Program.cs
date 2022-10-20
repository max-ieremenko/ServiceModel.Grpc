using System;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Service;
using ServiceModel.Grpc.Interceptors;
using Unity;

namespace NativeServiceHost;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var container = new UnityContainer();
        DebugModule.ConfigureContainer(container);

        var server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", SharedConfiguration.NativegRPCDebugServicePort, ServerCredentials.Insecure)
            }
        };

        server.Services.AddServiceModelTransient(
            container.Resolve<Func<DebugService>>(),
            options =>
            {
                // register server error handler
                options.ErrorHandler = container.Resolve<IServerErrorHandler>();
            });

        try
        {
            server.Start();

            Console.WriteLine("gRPC host is listening http:/localhost:{0}", server.Ports.First().Port);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
        finally
        {
            await server.ShutdownAsync();
        }

        Console.WriteLine("Press enter for exit...");
        Console.ReadLine();
    }
}