/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Compressor/Client/Program.cs
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
#if NETCOREAPP3_1
            ServiceModel.Grpc.GrpcChannelExtensions.Http2UnencryptedSupport = true;
#endif
            // 'grpc-internal-encoding-request' is a special metadata value that tells the client to compress the request.
            // This metadata is only used in the client is not sent as a header to the server.
            // The client sends 'Grpc-Encoding: gzip' header to the server
            var metadata = new Metadata
            {
                { "grpc-internal-encoding-request", "gzip" }
            };

            var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
            {
                // ask ServiceModel.Grpc to apply this metadata for any call from any client, created by this ClientFactory
                DefaultCallOptionsFactory = () => new CallOptions(metadata)
            });

            using var channel = GrpcChannel.ForAddress("http://localhost:5000");

            var client = clientFactory.CreateClient<IGreeterService>(channel);
            
            await UnaryCallExample(client);

            await ServerStreamingCallExample(client);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
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
}
