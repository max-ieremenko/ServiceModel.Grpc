/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Counter/Client/Program.cs
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client
{
    public static class Program
    {
        private static readonly Random Random = new Random();
        private static readonly IClientFactory ClientFactory = new ClientFactory();

        public static async Task Main(string[] args)
        {
#if NETCOREAPP3_1
            ServiceModel.Grpc.GrpcChannelExtensions.Http2UnencryptedSupport = true;
#endif

            using var channel = GrpcChannel.ForAddress("http://localhost:5000");

            var client = ClientFactory.CreateClient<ICounterService>(channel);
            
            await UnaryCallExample(client);

            await ClientStreamingCallExample(client);

            await ServerStreamingCallExample(client);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }
        }

        private static async Task UnaryCallExample(ICounterService client)
        {
            var reply = await client.IncrementCountAsync();
            Console.WriteLine("Count: " + reply);
        }

        private static async Task ClientStreamingCallExample(ICounterService client)
        {
            var count = await client.AccumulateCountAsync(GetAccumulateCountAmounts());
            Console.WriteLine($"Count: {count}");
        }

        private static async Task ServerStreamingCallExample(ICounterService client)
        {
            await foreach (var count in client.CountdownAsync())
            {
                Console.WriteLine($"Countdown: {count}");
            }
        }

        private static async IAsyncEnumerable<int> GetAccumulateCountAmounts()
        {
            for (var i = 0; i < 3; i++)
            {
                var count = Random.Next(5);
                Console.WriteLine($"Accumulating with {count}");
                yield return i;
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
