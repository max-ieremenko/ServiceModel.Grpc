using System;
using SimpleChat.Shared;
using System.Threading;

namespace SimpleChat.Server.Shared.Internal;

internal sealed class ConnectionLifetime
{
    private readonly string _userName;
    private readonly IChatServerRoom _room;
    private readonly IObserver<ChatMessage> _connection;

    private IDisposable? _connectionSubscription;
    private IDisposable? _requestTokenSubscription;
    private IDisposable? _shutdownTokenSubscription;

    public ConnectionLifetime(string userName, IChatServerRoom room, IObserver<ChatMessage> connection)
    {
        _userName = userName;
        _room = room;
        _connection = connection;
    }

    public void Subscribe(IObservable<ChatMessage> connections, CancellationToken requestToken, CancellationToken shutdownToken)
    {
        _connectionSubscription = connections.Subscribe(_connection);
        _requestTokenSubscription = requestToken.Register(OnUserDisconnect);
        _shutdownTokenSubscription = shutdownToken.Register(OnForceDisconnect);

        _room.BroadcastJoin(_userName);

        if (requestToken.IsCancellationRequested)
        {
            OnUserDisconnect();
        }
    }

    private void OnForceDisconnect()
    {
        _connectionSubscription?.Dispose();
        _requestTokenSubscription?.Dispose();
        _shutdownTokenSubscription?.Dispose();

        _connection.OnCompleted();
    }

    private void OnUserDisconnect()
    {
        OnForceDisconnect();

        _room.BroadcastLeft(_userName);
    }
}