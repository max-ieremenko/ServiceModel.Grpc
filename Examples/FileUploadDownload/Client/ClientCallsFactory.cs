using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client;

public readonly struct ClientCallsFactory
{
    private const string Http20ServerAddress = "http://localhost:5000";
    private const string Http11ServerAddress = "http://localhost:5001";

    public static HttpClientCalls CreateHttpClient(bool useCompression) => new HttpClientCalls(Http11ServerAddress, useCompression);

    public static GrpcClientCalls CreateGrpcClient(bool useCompression)
    {
        var channel = GrpcChannel.ForAddress(Http20ServerAddress);
        var fileService = CreateClientFactory(useCompression).CreateClient<IFileService>(channel);
        return new GrpcClientCalls(fileService);
    }

    private static IClientFactory CreateClientFactory(bool useCompression)
    {
        var options = new ServiceModelGrpcClientOptions
        {
            MarshallerFactory = DemoMarshallerFactory.Default
        };

        if (useCompression)
        {
            var headers = new Metadata
            {
                { "grpc-internal-encoding-request", CompressionSettings.Algorithm }
            };

            options.DefaultCallOptionsFactory = () => new CallOptions(headers);
        }

        return new ClientFactory(options);
    }
}