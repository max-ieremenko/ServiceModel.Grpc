using SimpleChat.Shared;

namespace SimpleChat.Server.Shared.Internal;

internal static class SystemBroadcast
{
    public static void BroadcastJoin(this IChatServerRoom room, string userName)
    {
        room.Broadcast(new ChatMessage
        {
            Author = userName,
            Content = "joined the chat"
        });
    }

    public static void BroadcastLeft(this IChatServerRoom room, string userName)
    {
        room.Broadcast(new ChatMessage
        {
            Author = userName,
            Content = "left the chat"
        });
    }

    public static void BroadcastShutdown(this IChatServerRoom room)
    {
        room.Broadcast(new ChatMessage
        {
            Author = "The administrator",
            Content = "closed the chart"
        });
    }
}