using Microsoft.AspNetCore.Components;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Blazor.Pages;

public partial class NotificationView
{
    [Parameter]
    [EditorRequired]
    public ChatNotification Notification { get; set; } = null!;
}