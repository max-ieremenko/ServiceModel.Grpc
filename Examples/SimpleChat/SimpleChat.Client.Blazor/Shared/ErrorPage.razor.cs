using System;
using Microsoft.AspNetCore.Components;

namespace SimpleChat.Client.Blazor.Shared;

public partial class ErrorPage
{
    [Parameter]
    [EditorRequired]
    public Exception Error { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    private void Reload() => Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
}