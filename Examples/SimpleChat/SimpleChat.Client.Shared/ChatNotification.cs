using System;
using SimpleChat.Shared;

namespace SimpleChat.Client.Shared;

public sealed record ChatNotification(
    DateTime Time,
    ChatNotificationSource Source,
    string Author,
    string Content)
{
    public string TimeAsText => Time.ToShortTimeString();

    public static ChatNotification FromMessage(ChatMessage message, string myConnectionId)
    {
        var source = ChatNotificationSource.OtherUser;

        if (string.IsNullOrEmpty(message.ConnectionId))
        {
            source = ChatNotificationSource.Administrator;
        }
        else if (message.ConnectionId == myConnectionId)
        {
            source = ChatNotificationSource.Me;
        }

        return new ChatNotification(
            message.Time.ToLocalTime(),
            source,
            message.Author,
            message.Content);
    }
}