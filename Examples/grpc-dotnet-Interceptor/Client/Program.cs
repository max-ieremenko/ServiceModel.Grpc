/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Client/Program.cs
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var clientFactory = new ClientFactory();

        using var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions());
        var invoker = channel.Intercept(new ClientLoggerInterceptor());

        var client = clientFactory.CreateClient<IGreeterService>(invoker);
            
        await UnaryCallExample(client);

        await ServerStreamingCallExample(client);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }

    private static async Task UnaryCallExample(IGreeterService client)
    {
        var reply = await client.SayHelloAsync("GreeterClient");
        Console.WriteLine("Greeting: " + reply);
    }

    private static async Task ServerStreamingCallExample(IGreeterService client)
    {
        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await foreach (var reply in client.SayHellosAsync("GreeterClient", tokenSource.Token))
            {
                Console.WriteLine("Greeting: " + reply);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // handle Status(StatusCode="Cancelled", Detail="Call canceled by the client.")
            Console.WriteLine("Call canceled by the client.");
        }
    }
}