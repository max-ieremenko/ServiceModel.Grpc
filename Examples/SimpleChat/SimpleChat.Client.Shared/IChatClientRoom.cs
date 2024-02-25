using System;
using System.Threading.Tasks;

namespace SimpleChat.Client.Shared;

public interface IChatClientRoom : IDisposable
{
    bool IsConnected { get; }

    Task JoinAsync(IObserver<ChatNotification> observer);

    Task MessageAsync(string content);
}