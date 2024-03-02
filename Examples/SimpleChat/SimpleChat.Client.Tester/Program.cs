using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Tester;

public static class Program
{
    public static async Task Main()
    {
        var ping = CreateChat("Ping", "http://localhost:8080", useHttp20: false);
        var pong = CreateChat("Pong", "http://localhost:8081", useHttp20: true);

        // Ping: joined the chat
        await ping.JoinAsync();
        await ping.WaitAsync(ChatNotificationSource.Administrator, ping.UserName, "joined the chat");

        // Pong: joined the chat
        await pong.JoinAsync();
        await ping.WaitAsync(ChatNotificationSource.Administrator, pong.UserName, "joined the chat");
        await pong.WaitAsync(ChatNotificationSource.Administrator, pong.UserName, "joined the chat");

        // Ping: Hello Pong
        await ping.MessageAsync($"Hello {pong.UserName}");
        await ping.WaitAsync(ChatNotificationSource.Me, ping.UserName, "Hello");
        await pong.WaitAsync(ChatNotificationSource.OtherUser, ping.UserName, "Hello");

        // Pong: Hello Ping
        await pong.MessageAsync($"Hello {ping.UserName}");
        await pong.WaitAsync(ChatNotificationSource.Me, pong.UserName, "Hello");
        await ping.WaitAsync(ChatNotificationSource.OtherUser, pong.UserName, "Hello");

        ping.Dispose();

        await pong.WaitAsync(ChatNotificationSource.Administrator, ping.UserName, "left the chat");

        pong.Dispose();

        ping.Errors.ShouldBeEmpty();
        pong.Errors.ShouldBeEmpty();
    }

    private static async Task WaitAsync(
        this Chat chat,
        ChatNotificationSource expectedSource,
        string expectedAuthor,
        string expectedContent)
    {
        var timer = Stopwatch.StartNew();

        while (chat.Notifications.Count == 0 && timer.Elapsed.TotalSeconds < 5)
        {
            await Task.Delay(100);
        }

        chat.Errors.ShouldBeEmpty();
        chat.Notifications.Count.ShouldBe(1, $"timeout {timer.Elapsed}");
        
        chat.Notifications[0].ShouldSatisfyAllConditions(
            i => i.Source.ShouldBe(expectedSource),
            i => i.Author.ShouldBe(expectedAuthor),
            i => i.Content.ShouldContain(expectedContent));

        chat.Notifications.Clear();
    }

    private static Chat CreateChat(string userName, string serverAddress, bool useHttp20)
    {
        var services = new ServiceCollection();

        if (useHttp20)
        {
            services.AddChatHttp20Client(_ => new Uri(serverAddress));
        }
        else
        {
            services.AddChatHttp11Client(_ => new Uri(serverAddress));
        }

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        return new Chat(serviceProvider, userName);
    }
}