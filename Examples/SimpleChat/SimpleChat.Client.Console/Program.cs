using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Client.Shared;
using SimpleChat.Shared;

namespace SimpleChat.Client.Console;

public static class Program
{
    public static async Task Main()
    {
        var serviceProvider = BuildServiceProvider();

        var room = await LoginAsync(serviceProvider);
        if (room == null)
        {
            return;
        }

        room.Join();
        await room.WaitForUserInputAsync();
    }

    private static async Task<Chat?> LoginAsync(IServiceProvider serviceProvider)
    {
        var output = new ConsoleOutput();

        var userName = output.Ask("Enter your name to join the chat:");
        if (userName == null)
        {
            return null;
        }

        var token = await serviceProvider.GetRequiredService<IAccountService>().ResolveTokenAsync(userName);
        serviceProvider.GetRequiredService<IJwtTokenProvider>().SetToken(token);

        var room = serviceProvider.GetRequiredService<IChatClientRoom>();
        return new Chat(output, room, userName);
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddChatHttp20Client(_ => new Uri("http://localhost:8081"));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}