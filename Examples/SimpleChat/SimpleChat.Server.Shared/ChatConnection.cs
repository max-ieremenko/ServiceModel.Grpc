using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using SimpleChat.Shared;

namespace SimpleChat.Server.Shared;

public sealed class ChatConnection : IObserver<ChatMessage>
{
    private const int MaxStuckMessages = 10;

    private readonly Channel<ChatMessage> _channel;

    public ChatConnection()
    {
        _channel = Channel.CreateBounded<ChatMessage>(new BoundedChannelOptions(MaxStuckMessages)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });
    }

    public void OnCompleted() => _channel.Writer.TryComplete();

    public void OnError(Exception error) => _channel.Writer.TryComplete(error);

    public void OnNext(ChatMessage value) => _channel.Writer.TryWrite(value);

    public async IAsyncEnumerable<ChatMessage> AsEnumerable([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            bool flag;
            try
            {
                flag = await _channel.Reader.WaitToReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                flag = false;
            }

            if (!flag)
            {
                break;
            }

            while (!cancellationToken.IsCancellationRequested && _channel.Reader.TryRead(out var message))
            {
                yield return message;
            }
        }
    }
}