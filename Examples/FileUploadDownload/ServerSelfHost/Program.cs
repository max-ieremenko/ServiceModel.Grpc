using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using Service;

namespace ServerSelfHost;

public static class Program
{
    public static async Task Main()
    {
        var server = BuildServer(false);
        server.Start();

        Console.WriteLine("...");
        Console.ReadLine();

        await server.ShutdownAsync();
    }

    public static Server BuildServer(bool useResponseCompression)
    {
        var channelOptions = new List<ChannelOption>();
        if (useResponseCompression)
        {
            channelOptions.Add(new ChannelOption(
                "grpc.default_compression_level",
                (int)TranslateCompressionLevel(CompressionSettings.Level)));
        }

        var server = new Server(channelOptions)
        {
            Ports = { new ServerPort("localhost", new Uri(Hosts.ServerSelfHost).Port, ServerCredentials.Insecure) }
        };

        server.Services.AddServiceModelTransient(
            () => new FileService(),
            options => options.MarshallerFactory = DemoMarshallerFactory.Default);
        server.Services.AddServiceModelTransient(
            () => new FileServiceRentedArray(),
            options => options.MarshallerFactory = DemoMarshallerFactory.Default);

        return server;
    }

    private static CompressionLevel TranslateCompressionLevel(System.IO.Compression.CompressionLevel level)
    {
        switch (level)
        {
            case System.IO.Compression.CompressionLevel.Optimal:
                return CompressionLevel.Medium;

            case System.IO.Compression.CompressionLevel.Fastest:
                return CompressionLevel.Low;
                
            default:
                return CompressionLevel.None;
        }
    }
}