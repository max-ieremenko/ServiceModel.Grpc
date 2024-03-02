using System;
using System.Net;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Client.DependencyInjection;
using SimpleChat.Client.Shared.Internal;
using SimpleChat.Shared;

namespace SimpleChat.Client.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatHttp11Client(this IServiceCollection services, Func<IServiceProvider, Uri> serverAddressResolver)
    {
        // register GrpcChannel
        services.AddSingleton(provider =>
        {
            var serverAddress = serverAddressResolver(provider);
            return CreateGrpcChannel(provider, serverAddress, useGrpcWeb: true);
        });

        ConfigureServices(services);
        return services;
    }

    public static IServiceCollection AddChatHttp20Client(this IServiceCollection services, Func<IServiceProvider, Uri> serverAddressResolver)
    {
        // register GrpcChannel
        services.AddSingleton(provider =>
        {
            var serverAddress = serverAddressResolver(provider);
            return CreateGrpcChannel(provider, serverAddress, useGrpcWeb: false);
        });

        ConfigureServices(services);
        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

        services
            .AddServiceModelGrpcClient<IChatService>()
            .AddServiceModelGrpcClient<IAccountService>();

        services.AddTransient<IChatClientRoom, ChatClientRoom>();
    }

    private static ChannelBase CreateGrpcChannel(IServiceProvider serviceProvider, Uri serverAddress, bool useGrpcWeb)
    {
        var tokenProvider = serviceProvider.GetRequiredService<IJwtTokenProvider>();

        HttpMessageHandler httpHandler = new HttpMessageHandlerWithAuthorization(tokenProvider);
        if (useGrpcWeb)
        {
            httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, httpHandler)
            {
                HttpVersion = HttpVersion.Version11
            };
        }

        var options = new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        };

        return GrpcChannel.ForAddress(serverAddress, options);
    }
}