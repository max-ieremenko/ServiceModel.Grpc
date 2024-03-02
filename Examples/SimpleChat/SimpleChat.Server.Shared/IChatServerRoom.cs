using System;
using System.Threading;
using SimpleChat.Shared;

namespace SimpleChat.Server.Shared;

public interface IChatServerRoom
{
    void Broadcast(ChatMessage message);

    void Join(string userName, IObserver<ChatMessage> connection, CancellationToken cancellationToken);
}