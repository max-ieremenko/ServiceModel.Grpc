using System;
using System.Threading.Tasks;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Console;

internal sealed class Chat : IObserver<ChatNotification>
{
    private readonly ConsoleOutput _output;
    private readonly IChatClientRoom _room;
    private readonly string _userName;

    public Chat(ConsoleOutput output, IChatClientRoom room, string userName)
    {
        _output = output;
        _room = room;
        _userName = userName;
    }

    public async void Join()
    {
        await _room.JoinAsync(this);
    }

    public async Task WaitForUserInputAsync()
    {
        while (true)
        {
            var message = _output.Ask($"{_userName}:");
            if (message == null || !_room.IsConnected)
            {
                _room.Dispose();
                return;
            }

            await _room.MessageAsync(message);
        }
    }

    public void OnCompleted() => _output.AppendLine("Chat connection lost");

    public void OnError(Exception error) => _output.AppendLine($"Error: {error.Message}");

    public void OnNext(ChatNotification value)
    {
        var author = value.Source == ChatNotificationSource.Me ? "Me" : value.Author;
        _output.AppendLine($"{value.TimeAsText} {author}: {value.Content}");
    }
}