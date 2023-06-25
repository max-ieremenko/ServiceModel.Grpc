using System;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client;

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

        AddClientFactoryWithAuthorization(services);
        AddDemoService(services);
        AddTokenProvider(services);

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildWithGrpcNetChannel()
    {
        var services = new ServiceCollection();

        // register Grpc.Net.Client.GrpcChannel
        services.AddSingleton<ChannelBase>(_ => GrpcChannel.ForAddress(ServerAddress, new GrpcChannelOptions()));

        AddClientFactoryWithAuthorization(services);
        AddDemoService(services);
        AddTokenProvider(services);

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildWithAuthorizationMessageHandler()
    {
        var services = new ServiceCollection();

        // register Grpc.Net.Client.GrpcChannel
        services.AddSingleton<ChannelBase>(provider =>
        {
            var handler = new HttpMessageHandlerWithAuthorization(provider.GetRequiredService<IJwtTokenProvider>());
            var options = new GrpcChannelOptions { HttpHandler = handler };

            return GrpcChannel.ForAddress(ServerAddress, options);
        });

        // register ClientFactory with default options
        services.AddSingleton<IClientFactory, ClientFactory>();

        AddDemoService(services);
        AddTokenProvider(services);

        return services.BuildServiceProvider();
    }

    private static void AddDemoService(IServiceCollection services)
    {
        // resolve IDemoService from ClientFactory
        services.AddTransient<IDemoService>(provider =>
        {
            var channel = provider.GetRequiredService<ChannelBase>();
            var clientFactory = provider.GetRequiredService<IClientFactory>();
            return clientFactory.CreateClient<IDemoService>(channel);
        });
    }

    private static void AddTokenProvider(IServiceCollection services)
    {
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
    }

    private static void AddClientFactoryWithAuthorization(IServiceCollection services)
    {
        // register ClientFactory with Authorization
        services.AddSingleton<IClientFactory>(provider =>
        {
            var callOptionsFactory = new CallOptionsFactoryWithAuthorization(provider.GetRequiredService<IJwtTokenProvider>());

            // setup authorization header for all calls, by all proxies created by this factory
            var options = new ServiceModelGrpcClientOptions { DefaultCallOptionsFactory = callOptionsFactory.Create };

            return new ClientFactory(options);
        });
    }
}