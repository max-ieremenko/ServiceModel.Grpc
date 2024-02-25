using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Client.Shared;
using SimpleChat.Shared;

namespace SimpleChat.Client.Tester;

internal sealed class Chat : IObserver<ChatNotification>, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IChatClientRoom _room;

    public Chat(IServiceProvider serviceProvider, string userName)
    {
        UserName = userName;
        _serviceProvider = serviceProvider;
        _room = serviceProvider.GetRequiredService<IChatClientRoom>();
        Notifications = new();
        Errors = new();
    }

    public string UserName { get; }

    public List<ChatNotification> Notifications { get; }

    public List<Exception> Errors { get; }

    public async Task JoinAsync()
    {
        var token = await _serviceProvider.GetRequiredService<IAccountService>().ResolveTokenAsync(UserName);
        _serviceProvider.GetRequiredService<IJwtTokenProvider>().SetToken(token);

        _ = _room.JoinAsync(this);
    }

    public Task MessageAsync(string content) => _room.MessageAsync(content);

    void IObserver<ChatNotification>.OnCompleted()
    {
    }

    void IObserver<ChatNotification>.OnError(Exception error) => Errors.Add(error);

    void IObserver<ChatNotification>.OnNext(ChatNotification value) => Notifications.Add(value);

    public void Dispose() => _room.Dispose();
}
