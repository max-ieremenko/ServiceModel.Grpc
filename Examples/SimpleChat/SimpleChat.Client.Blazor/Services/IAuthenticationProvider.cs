namespace SimpleChat.Client.Blazor.Services;

public interface IAuthenticationProvider
{
    string? CurrentUserName { get; }

    void OnLogin(string userName, string token);

    void OnLogout();
}