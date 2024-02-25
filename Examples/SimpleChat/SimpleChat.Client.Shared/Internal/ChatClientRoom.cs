using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using SimpleChat.Shared;

namespace SimpleChat.Client.Shared.Internal;

internal sealed class ChatClientRoom : IChatClientRoom
{
    private readonly IChatService _chat;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly string _myConnectionId;

    private IObserver<ChatNotification>? _observer;

    public ChatClientRoom(IChatService chat)
    {
        _chat = chat;
        _myConnectionId = Guid.NewGuid().ToString();
        _cancellationSource = new CancellationTokenSource();
    }

    public bool IsConnected { get; private set; }

    public async Task JoinAsync(IObserver<ChatNotification> observer)
    {
        _observer = observer;

        try
        {
            var stream = _chat.JoinAsync(_cancellationSource.Token);
            
            IsConnected = true;
            
            await foreach (var message in stream)
            {
                OnNewMessage(message);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
        }
        catch (RpcException)
        {
            // server crash
        }

        await _cancellationSource.CancelAsync();
        OnDisconnect();
    }

    public async Task MessageAsync(string content)
    {
        if (_cancellationSource.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await _chat.MessageAsync(_myConnectionId, content, _cancellationSource.Token);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
        }
        catch (RpcException ex)
        {
            _observer?.OnError(ex);
        }
    }

    public void Dispose() => _cancellationSource.Cancel();

    private void OnNewMessage(ChatMessage message)
    {
        var notification = ChatNotification.FromMessage(message, _myConnectionId);
        _observer?.OnNext(notification);
    }

    private void OnDisconnect()
    {
        IsConnected = false;
        _observer?.OnCompleted();
    }
}