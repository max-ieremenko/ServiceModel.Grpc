using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Blazor.Services;

internal sealed class AuthenticationProvider : AuthenticationStateProvider, IAuthenticationProvider
{
    private readonly IJwtTokenProvider _tokenProvider;
    private ClaimsPrincipal _currentUser;

    public AuthenticationProvider(IJwtTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        _currentUser = CreatePrincipal(null);
    }

    public string? CurrentUserName => _currentUser.Identity?.Name;

    public void OnLogin(string userName, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        _tokenProvider.SetToken(token);
        _currentUser = CreatePrincipal(userName);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void OnLogout()
    {
        _tokenProvider.SetToken(null);
        _currentUser = CreatePrincipal(null);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = new AuthenticationState(_currentUser);
        return Task.FromResult(state);
    }

    private static ClaimsPrincipal CreatePrincipal(string? userName)
    {
        ClaimsIdentity identity;
        if (string.IsNullOrWhiteSpace(userName))
        {
            identity = new ClaimsIdentity();
        }
        else
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName)
            };

            identity = new ClaimsIdentity(claims, "demo");
        }

        return new ClaimsPrincipal(identity);
    }
}