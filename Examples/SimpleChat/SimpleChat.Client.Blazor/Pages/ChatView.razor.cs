using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SimpleChat.Client.Blazor.Services;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Blazor.Pages;

public partial class ChatView : IObserver<ChatNotification>, IDisposable
{
    private readonly List<ChatNotification> _notifications = new();
    private string? _newMessageContent;

    [Inject]
    public IAuthenticationProvider AuthenticationProvider { get; set; } = null!;

    [Inject]
    public IChatClientRoom Room { get; set; } = null!;

    public void Dispose() => Room.Dispose();

    protected override Task OnInitializedAsync() => Room.JoinAsync(this);

    private void Disconnect() => AuthenticationProvider.OnLogout();

    private async Task SendAsync()
    {
        if (!string.IsNullOrEmpty(_newMessageContent))
        {
            await Room.MessageAsync(_newMessageContent);
        }

        _newMessageContent = null;
    }

    public void OnCompleted()
    {
        _newMessageContent = "Chat connection lost";
        StateHasChanged();
    }

    public void OnError(Exception error) => throw error;

    public void OnNext(ChatNotification value)
    {
        _notifications.Add(value);
        StateHasChanged();
    }
}