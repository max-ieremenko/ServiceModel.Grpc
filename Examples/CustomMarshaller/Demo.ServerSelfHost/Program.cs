using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client;
using CustomMarshaller;
using Grpc.Core;
using Service;

namespace Demo.ServerSelfHost
{
    public static class Program
    {
        public static async Task Main()
        {
            var server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", 5001, ServerCredentials.Insecure)
                }
            };

            server.Services.AddServiceModelSingleton(
                new PersonService(),
                options =>
                {
                    // set JsonMarshallerFactory as default Marshaller
                    options.MarshallerFactory = JsonMarshallerFactory.Default;
                });

            server.Start();

            await ClientCalls.Run(5001);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            await server.ShutdownAsync();
        }
    }
}
