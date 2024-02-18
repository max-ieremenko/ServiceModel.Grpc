namespace Client.Services;

internal interface IJwtTokenProvider
{
    string? GetToken();
}