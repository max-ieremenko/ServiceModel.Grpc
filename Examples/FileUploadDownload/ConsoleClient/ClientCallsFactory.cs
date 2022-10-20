using System;
using System.Net.Http;
using ConsoleClient.Internal;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using ServiceModel.Grpc.Client;

namespace ConsoleClient;

public enum ChannelType { GrpcCore, GrpcNet }

public readonly struct ClientCallsFactory
{
    private readonly ServiceModelGrpcClientOptions _clientOptions;
    private readonly Func<ChannelBase> _channelFactory;

    public ClientCallsFactory(Func<ChannelBase> channelFactory)
    {
        _channelFactory = channelFactory;
        _clientOptions = new ServiceModelGrpcClientOptions
        {
            MarshallerFactory = DemoMarshallerFactory.Default
        };
    }

    public static IClientCalls CreateHttpClient(bool useCompression)
    {
        return new HttpClientCalls(Hosts.ServerAspNetHostHttp1, useCompression);
    }

    public static ClientCallsFactory ForAspNetHost(GrpcWebMode mode)
    {
        Func<ChannelBase> channelFactory = () =>
        {
            var options = new GrpcChannelOptions
            {
                DisposeHttpClient = true,
                HttpHandler = new GrpcWebHandler(mode, new HttpClientHandler())
                {
                    HttpVersion = System.Net.HttpVersion.Version11
                }
            };

            return GrpcChannel.ForAddress(Hosts.ServerAspNetHostHttp1, options);
        };

        return new ClientCallsFactory(channelFactory);
    }

    public static ClientCallsFactory ForAspNetHost(ChannelType type)
    {
        return ForChannel(Hosts.ServerAspNetHostHttp2, type);
    }

    public static ClientCallsFactory ForSelfHost(ChannelType type)
    {
        return ForChannel(Hosts.ServerSelfHost, type);
    }

    public static ClientCallsFactory ForChannel(string address, ChannelType type)
    {
        Func<ChannelBase> channelFactory = () =>
        {
            if (type == ChannelType.GrpcNet)
            {
                return GrpcChannel.ForAddress(
                    address,
                    new GrpcChannelOptions { DisposeHttpClient = true });
            }

            var url = new Uri(address);
            return new Channel(url.Host, url.Port, ChannelCredentials.Insecure);
        };

        return new ClientCallsFactory(channelFactory);
    }

    public ClientCallsFactory WithCompression(bool useCompression)
    {
        if (useCompression)
        {
            var headers = new Metadata
            {
                { "grpc-internal-encoding-request", CompressionSettings.Algorithm }
            };

            _clientOptions.DefaultCallOptionsFactory = () => new CallOptions(headers);
        }
        else
        {
            _clientOptions.DefaultCallOptionsFactory = null;
        }

        return this;
    }

    public IClientCalls CreateFileService()
    {
        var channel = _channelFactory();
        var fileService = new ClientFactory(_clientOptions).CreateClient<IFileService>(channel);
        return new FileServiceClientCalls(channel, fileService);
    }

    public IClientCalls CreateFileServiceRentedArray()
    {
        var channel = _channelFactory();
        var fileService = new ClientFactory(_clientOptions).CreateClient<IFileServiceRentedArray>(channel);
        return new FileServiceRentedArrayClientCalls(channel, fileService);
    }
}