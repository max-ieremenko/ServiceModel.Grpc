using System;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceModel.Grpc.Client.DependencyInjection;

namespace Client.ReflectionEmit;

public static class DemoGrpcNetChannel
{
    public static void ConfigureServices1(IServiceCollection services)
    {
        // provide Channel
        services.AddSingleton(CreateChannel);

        // optional ClientFactory configuration
        services
            .AddServiceModelGrpcClientFactory((factoryOptions, provider) =>
            {
                // ...
            })
            .AddClient<ICalculator>((clientOptions, provider) =>
            {
                // optional ICalculator proxy configuration
            });

        services.AddServiceModelGrpcClient<IRandomNumberGenerator>();
    }

    public static void ConfigureServices2(IServiceCollection services)
    {
        services
            .AddServiceModelGrpcClientFactory()
            .ConfigureDefaultChannel(ChannelProviderFactory.Transient(CreateChannel)) // provide Channel
            .AddClient<ICalculator>();

        services.AddServiceModelGrpcClient<IRandomNumberGenerator>((clientOptions, provider) =>
        {
            // optional IRandomNumberGenerator proxy configuration
        });
    }

    public static void ConfigureServices3(IServiceCollection services)
    {
        services.AddServiceModelGrpcClient<ICalculator>(
            channel: ChannelProviderFactory.Transient(CreateChannel));

        services.AddServiceModelGrpcClient<IRandomNumberGenerator>(
            channel: ChannelProviderFactory.Transient(CreateChannel));
    }

    private static ChannelBase CreateChannel(IServiceProvider serviceProvider)
    {
        var serverAddress = serviceProvider.GetRequiredService<IOptions<ClientConfiguration>>().Value.ServerAddress;
        return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions());
    }
}