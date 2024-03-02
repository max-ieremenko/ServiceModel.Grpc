using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SimpleChat.Client.Blazor.Services;
using SimpleChat.Shared;

namespace SimpleChat.Client.Blazor.Pages;

public partial class ChatLoginView
{
    private string? _userName;

    [Inject]
    public IAuthenticationProvider AuthenticationProvider { get; set; } = null!;

    [Inject]
    public IAccountService AccountService { get; set; } = null!;

    private async Task JoinAsync()
    {
        if (string.IsNullOrWhiteSpace(_userName))
        {
            return;
        }

        var token = await AccountService.ResolveTokenAsync(_userName);
        AuthenticationProvider.OnLogin(_userName, token);
    }
}