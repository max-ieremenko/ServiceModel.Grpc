using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SimpleChat.Shared;

namespace SimpleChat.Server.Services;

internal sealed class AccountService : IAccountService
{
    // dummy key, only for demo purposes
    public static SecurityKey GetDummyKey() => new SymmetricSecurityKey(new byte[256 / 8]);

    public Task<string> ResolveTokenAsync(string userName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = nameof(ChatService),
            Expires = DateTime.UtcNow.AddHours(2),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }),
            SigningCredentials = new SigningCredentials(GetDummyKey(), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return Task.FromResult(handler.WriteToken(token));
    }
}