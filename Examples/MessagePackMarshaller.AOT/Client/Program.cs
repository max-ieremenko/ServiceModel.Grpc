using System;
using System.Threading.Tasks;
using Client.Services;
using Contract;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Client.DependencyInjection;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSimpleConsole());
        services.AddTransient<ClientTestFixture>();

        AddApplicationServices(services, new Uri("http://localhost:5000"));

        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<ClientTestFixture>().RunAsync();
    }

    private static void AddApplicationServices(IServiceCollection services, Uri serverAddress)
    {
        services.AddTransient<LoggingClientFilter>();

        services
            .AddServiceModelGrpcClientFactory((options, provider) =>
            {
                // set MessagePackMarshaller with generated formatters as default Marshaller
                options.MarshallerFactory = new MessagePackMarshallerFactory(MessagePackSerializerHelper.CreateApplicationOptions());

                // Filters: log gRPC calls
                options.Filters.Add(1, provider.GetRequiredService<LoggingClientFilter>());

                // Error handling: activate ServerErrorHandler
                options.ErrorHandler = new ClientErrorHandlerCollection(new ClientErrorHandler());

                // Error handling: AOT compatible marshalling of InvalidRectangleError
                options.ErrorDetailDeserializer = new ClientFaultDetailDeserializer();
            })
            .ConfigureDefaultChannel(ChannelProviderFactory.Singleton(GrpcChannel.ForAddress(serverAddress)))
            .AddAllGrpcServices();
    }
}