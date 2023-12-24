using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client.Services;
using Contract;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        // demo with Grpc.Core.Channel and ServiceModelGrpcClientOptions.DefaultCallOptionsFactory
        Console.WriteLine("--- Grpc.Core.Channel ---");
        await using (var serviceProvider = ServiceProviderBuilder.BuildWithGrpcCoreChannel())
        {
            await Run(serviceProvider);
        }

        // demo with Grpc.Net.Client.GrpcChannel and ServiceModelGrpcClientOptions.DefaultCallOptionsFactory
        Console.WriteLine("--- Grpc.Net.Client.GrpcChannel ---");
        await using (var serviceProvider = ServiceProviderBuilder.BuildWithGrpcNetChannel())
        {
            await Run(serviceProvider);
        }

        // demo with Grpc.Net.Client.GrpcChannel and HttpMessageHandlerWithAuthorization
        Console.WriteLine("--- HttpMessageHandlerWithAuthorization 1 ---");
        await using (var serviceProvider = ServiceProviderBuilder.BuildWithAuthorizationMessageHandler1())
        {
            await Run(serviceProvider);
        }

        // demo with Grpc.Net.Client.GrpcChannel and HttpMessageHandlerWithAuthorization
        Console.WriteLine("--- HttpMessageHandlerWithAuthorization 2 ---");
        await using (var serviceProvider = ServiceProviderBuilder.BuildWithAuthorizationMessageHandler2())
        {
            await Run(serviceProvider);
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task Run(IServiceProvider serviceProvider)
    {
        var tokenProvider = (JwtTokenProvider)serviceProvider.GetRequiredService<IJwtTokenProvider>();
        var demoServiceProxy = serviceProvider.GetRequiredService<IDemoService>();

        tokenProvider.SetCurrentUser(null);
        await CallAsAnonymous(demoServiceProxy);

        tokenProvider.SetCurrentUser("demo-user");
        await CallAsDemoUser(demoServiceProxy);
    }

    private static async Task CallAsAnonymous(IDemoService demoServiceProxy)
    {
        Console.WriteLine("Invoke PingAsync");
        var response = await demoServiceProxy.PingAsync();
        Console.WriteLine(response);
        response.ShouldBe("pong <unauthorized>");
    }

    private static async Task CallAsDemoUser(IDemoService demoServiceProxy)
    {
        Console.WriteLine("Invoke GetCurrentUserNameAsync");
        var userName = await demoServiceProxy.GetCurrentUserNameAsync();
        Console.WriteLine(userName);
        userName.ShouldBe("demo-user");

        Console.WriteLine("Invoke PingAsync");
        var response = await demoServiceProxy.PingAsync();
        Console.WriteLine(response);
        response.ShouldBe("pong demo-user");
    }
}