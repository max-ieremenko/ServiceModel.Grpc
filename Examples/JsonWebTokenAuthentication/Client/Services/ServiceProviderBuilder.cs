using System;
using System.Net.Http;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.DependencyInjection;

namespace Client.Services;

internal static class ServiceProviderBuilder
{
    private const string ServerAddress = "http://localhost:8080";

    public static ServiceProvider BuildWithGrpcCoreChannel()
    {
        var services = new ServiceCollection();

        // register Grpc.Core.Channel
        services.AddSingleton<ChannelBase>(_ =>
        {
            var address = new Uri(ServerAddress);
            return new Channel(address.Host, address.Port, ChannelCredentials.Insecure);
        });

        services
            .AddTokenProvider()
            .AddClientFactoryWithAuthorization()
            .AddClient<IDemoService>();

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildWithGrpcNetChannel()
    {
        var services = new ServiceCollection();

        // Grpc.Net.Client.GrpcChannel
        services.AddSingleton<ChannelBase>(_ => GrpcChannel.ForAddress(ServerAddress, new GrpcChannelOptions()));

        services
            .AddTokenProvider()
            .AddClientFactoryWithAuthorization()
            .AddClient<IDemoService>();

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildWithAuthorizationMessageHandler1()
    {
        var services = new ServiceCollection();

        // Grpc.Net.Client.GrpcChannel
        services.AddSingleton<ChannelBase>(provider =>
        {
            var handler = new HttpMessageHandlerWithAuthorization(
                new HttpClientHandler(),
                provider.GetRequiredService<IJwtTokenProvider>());

            var options = new GrpcChannelOptions { HttpHandler = handler };

            return GrpcChannel.ForAddress(ServerAddress, options);
        });

        services
            .AddTokenProvider()
            .AddClientFactoryWithAuthorization()
            .AddClient<IDemoService>();

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildWithAuthorizationMessageHandler2()
    {
        var services = new ServiceCollection();

        // register Grpc.Net.Client.GrpcChannel
        services
            .AddGrpcClient<IDemoService>(options =>
            {
                options.Address = new Uri(ServerAddress);
            })
            .AddHttpMessageHandler(provider =>
            {
                return new HttpMessageHandlerWithAuthorization(provider.GetRequiredService<IJwtTokenProvider>());
            })
            .ConfigureServiceModelGrpcClientCreator<IDemoService>();

        AddTokenProvider(services);

        return services.BuildServiceProvider();
    }

    private static IServiceCollection AddTokenProvider(this IServiceCollection services) => services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

    private static IClientFactoryBuilder AddClientFactoryWithAuthorization(this IServiceCollection services)
    {
        // register ClientFactory with Authorization
        return services.AddServiceModelGrpcClientFactory((options, provider) =>
        {
            var callOptionsFactory = new CallOptionsFactoryWithAuthorization(provider.GetRequiredService<IJwtTokenProvider>());
            options.DefaultCallOptionsFactory = callOptionsFactory.Create;
        });
    }
}