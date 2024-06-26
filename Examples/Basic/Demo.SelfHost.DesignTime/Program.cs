﻿using System.Threading.Tasks;
using Client;
using Grpc.Core;
using Service;

namespace Demo.SelfHost.DesignTime;

public static class Program
{
    private const int Port = 8083;

    public static async Task Main()
    {
        var server = new Server
        {
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        // host PersonService endpoint generated by ServiceModel.Grpc.DesignTime
        server.Services.AddPersonService(() => new PersonService());

        server.Start();

        try
        {
            var clientCalls = new ClientCalls();

            // register IPersonService proxy generated by ServiceModel.Grpc.DesignTime
            clientCalls.ClientFactory.AddPersonServiceClient();

            await clientCalls.CallPersonService(Port);
        }
        finally
        {
            await server.ShutdownAsync();
        }
    }
}