namespace SimpleChat.Client.Shared.Internal;

internal sealed class JwtTokenProvider : IJwtTokenProvider
{
    private string? _currentToken;

    public string? GetToken() => _currentToken;

    public void SetToken(string? token)
    {
        _currentToken = token;
    }
}