using Contract;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Service;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSelfHost;

public class ServerHost : IHostedService
{
    private readonly Server _server;

    public ServerHost()
    {
        _server = BuildServer(false);
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();

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