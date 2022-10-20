using System;
using Contract;
using Grpc.Core;

namespace ServerNativeHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", ServiceConfiguration.ServiceNativeGrpcPort, ServerCredentials.Insecure)
            }
        };

        server.Services.Add(CalculatorNative.BindService(new CalculatorService()));

        server.Start();

        Console.WriteLine("Press enter for exit...");
        Console.ReadLine();
    }
}