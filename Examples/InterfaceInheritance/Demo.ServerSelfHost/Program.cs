using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client;
using Grpc.Core;
using Service;

namespace Demo.ServerSelfHost;

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

        // register generated DoubleCalculatorEndpoint, see MyGrpcServices
        server.Services.AddDoubleCalculator(new DoubleCalculator());

        // endpoint will be generated on-fly
        server.Services.AddServiceModelSingleton(new GenericCalculator<int>());

        server.Start();

        var calls = new ClientCalls(5001);

        await calls.InvokeGenericCalculator();
        await calls.InvokeDoubleCalculator();

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }

        await server.ShutdownAsync();
    }
}