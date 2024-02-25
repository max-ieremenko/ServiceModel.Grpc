namespace SimpleChat.Client.Shared;

public interface IJwtTokenProvider
{
    string? GetToken();

    void SetToken(string? token);
}