using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Service;
using Unity;

namespace NativeServiceHost
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var container = new UnityContainer();
            PersonModule.ConfigureContainer(container);

            var server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", SharedConfiguration.NativegRPCPersonServicePort, ServerCredentials.Insecure)
                }
            };

            server.Services.AddServiceModelTransient(container.Resolve<Func<PersonService>>());

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
}
