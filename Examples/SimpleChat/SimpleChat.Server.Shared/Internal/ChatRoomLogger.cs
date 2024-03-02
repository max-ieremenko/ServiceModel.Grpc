using System;
using Microsoft.Extensions.Logging;
using SimpleChat.Shared;

namespace SimpleChat.Server.Shared.Internal;

internal sealed class ChatRoomLogger : IObserver<ChatMessage>
{
    private readonly ILogger _logger;

    public ChatRoomLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void OnCompleted() => _logger.LogInformation("Shutdown");

    public void OnError(Exception error) => _logger.LogError(error, message: "Error");

    public void OnNext(ChatMessage value)
    {
        var source = value.ConnectionId ?? "system";
        _logger.LogInformation($"{value.Author} ({source}): {value.Content}");
    }
}