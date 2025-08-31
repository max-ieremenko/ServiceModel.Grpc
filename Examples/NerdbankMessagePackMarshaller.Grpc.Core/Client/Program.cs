using Client.Services;
using Contract;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using System.Threading.Tasks;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    private const int Port = 8082;

    public static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSimpleConsole());
        services.AddTransient<ClientTestFixture>();

        AddApplicationServices(services);

        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<ClientTestFixture>().RunAsync();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddTransient<LoggingClientFilter>();

        services.AddSingleton<ChannelBase>(new Channel("localhost", Port, ChannelCredentials.Insecure));

        services.AddSingleton<IClientFactory>(provider =>
        {
            var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
            {
                // set NerdbankMessagePackMarshaller with generated formatters as default Marshaller
                MarshallerFactory = new NerdbankMessagePackMarshallerFactory(PolyTypes.TypeShapeProvider),

                // Filters: log gRPC calls
                Filters =
                {
                    { 1, provider.GetRequiredService<LoggingClientFilter>() }
                },

                // Error handling: activate ServerErrorHandler
                ErrorHandler = new ClientErrorHandlerCollection(new ClientErrorHandler())
            });

            // see Services.GrpcServices.cs
            clientFactory.AddCalculatorClient();
            return clientFactory;
        });

        services.AddTransient<ICalculator>(provider =>
        {
            var channel = provider.GetRequiredService<ChannelBase>();
            return provider.GetRequiredService<IClientFactory>().CreateClient<ICalculator>(channel);
        });
    }
}