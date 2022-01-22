using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client;
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

            server.Services.AddServiceModelSingleton(new PersonService());

            server.Start();

            var calls = new ClientCalls(5001);

            calls.RunSync();
            await calls.RunAsync();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            await server.ShutdownAsync();
        }
    }
}
