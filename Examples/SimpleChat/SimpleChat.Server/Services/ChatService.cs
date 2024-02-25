using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SimpleChat.Server.Shared;
using SimpleChat.Shared;

namespace SimpleChat.Server.Services;

[Authorize]
internal sealed class ChatService : IChatService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IChatServerRoom _chatRoom;

    public ChatService(IHttpContextAccessor contextAccessor, IChatServerRoom chatRoom)
    {
        _contextAccessor = contextAccessor;
        _chatRoom = chatRoom;
    }

    public Task MessageAsync(string connectionId, string content, CancellationToken cancellationToken)
    {
        var message = new ChatMessage
        {
            Author = GetCurrentUserName(),
            ConnectionId = connectionId,
            Content = content
        };

        _chatRoom.Broadcast(message);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ChatMessage> JoinAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            throw new ArgumentOutOfRangeException(nameof(cancellationToken));
        }

        var connection = new ChatConnection();
        _chatRoom.Join(GetCurrentUserName(), connection, cancellationToken);

        return connection.AsEnumerable(cancellationToken);
    }

    private string GetCurrentUserName() => _contextAccessor.HttpContext!.User.Identity!.Name!;
}