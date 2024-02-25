using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleChat.Shared;

namespace SimpleChat.Server.Shared.Internal;

internal sealed class ChatServerRoom : IChatServerRoom
{
    private readonly BufferBlock<ChatMessage> _messages;
    private readonly IObservable<ChatMessage> _connections;
    private readonly CancellationTokenSource _shutdownSource;

    public ChatServerRoom(IHostApplicationLifetime appLifetime, ILoggerFactory loggerFactory)
    {
        _messages = new BufferBlock<ChatMessage>(new DataflowBlockOptions
        {
            EnsureOrdered = true
        });

        _connections = _messages.AsObservable();
        _connections.Subscribe(new ChatRoomLogger(loggerFactory.CreateLogger(nameof(ChatServerRoom))));

        _shutdownSource = new CancellationTokenSource();
        appLifetime.ApplicationStopping.Register(Shutdown);
    }

    public void Broadcast(ChatMessage message)
    {
        if (!_shutdownSource.IsCancellationRequested)
        {
            message.Time = DateTime.UtcNow;
            _messages.Post(message);
        }
    }

    public void Join(string userName, IObserver<ChatMessage> connection, CancellationToken cancellationToken)
    {
        var lifetime = new ConnectionLifetime(userName, this, connection);
        lifetime.Subscribe(_connections, cancellationToken, _shutdownSource.Token);
    }

    private void Shutdown()
    {
        this.BroadcastShutdown();

        _shutdownSource.Cancel();
        _messages.Complete();
    }
}