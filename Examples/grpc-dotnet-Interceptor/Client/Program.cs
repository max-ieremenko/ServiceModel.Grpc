/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Client/Program.cs
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var clientFactory = new ClientFactory();

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        using var channel = GrpcChannel.ForAddress(
            "http://localhost:5000",
            new GrpcChannelOptions
            {
                LoggerFactory = loggerFactory
            });

        var invoker = channel.Intercept(new ClientLoggerInterceptor(loggerFactory));

        var client = clientFactory.CreateClient<IGreeterService>(invoker);

        BlockingUnaryCallExample(client);

        await UnaryCallExample(client);

        await ServerStreamingCallExample(client);

        await ClientStreamingCallExample(client);

        await BidirectionalCallExample(client);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }

    private static void BlockingUnaryCallExample(IGreeterService client)
    {
        var reply = client.SayHello("GreeterClient");
        Console.WriteLine("Greeting: " + reply);
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

    static async Task ClientStreamingCallExample(IGreeterService client)
    {
        static async IAsyncEnumerable<string> GetNames()
        {
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(1);
                yield return $"GreeterClient{i + 1}";
            }
        }

        var reply = await client.SayHelloToLotsOfBuddiesAsync(GetNames(), default);
        Console.WriteLine("Greeting: " + reply);
    }

    static async Task BidirectionalCallExample(IGreeterService client)
    {
        static async IAsyncEnumerable<string> GetNames()
        {
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(1);
                yield return $"GreeterClient{i + 1}";
            }
        }

        var messages = client.SayHellosToLotsOfBuddiesAsync(GetNames(), default);
        await foreach (var message in messages)
        {
            Console.WriteLine("Greeting: " + message);
        }
    }
}