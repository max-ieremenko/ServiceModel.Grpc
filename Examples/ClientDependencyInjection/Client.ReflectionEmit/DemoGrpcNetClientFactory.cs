using Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Client.DependencyInjection;

namespace Client.ReflectionEmit;

public static class DemoGrpcNetClientFactory
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // optional ClientFactory configuration
        services
            .AddServiceModelGrpcClientFactory((factoryOptions, provider) =>
            {
                // ...
            });

        services
            .AddGrpcClient<ICalculator>((provider, options) =>
            {
                var serverAddress = provider.GetRequiredService<IOptions<ClientConfiguration>>().Value.ServerAddress;
                options.Address = serverAddress;
            })
            .ConfigureServiceModelGrpcClientCreator<ICalculator>((clientOptions, provider) =>
            {
                // optional: configure ICalculator proxy
            });

        services
            .AddGrpcClient<IRandomNumberGenerator>((provider, options) =>
            {
                var serverAddress = provider.GetRequiredService<IOptions<ClientConfiguration>>().Value.ServerAddress;
                options.Address = serverAddress;
            })
            .ConfigureServiceModelGrpcClientCreator<IRandomNumberGenerator>();
    }
}