using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Client.Services;

internal sealed class JwtTokenProvider : IJwtTokenProvider
{
    public string CurrentUser { get; private set; }

    public string GetToken()
    {
        if (CurrentUser == null)
        {
            return null;
        }

        return CreateToken(CurrentUser);
    }

    internal void SetCurrentUser(string userName)
    {
        CurrentUser = string.IsNullOrEmpty(userName) ? null : userName;
    }

    private static string CreateToken(string userName)
    {
        // dummy key, only for demo purposes, must be synchronized with WebApplication
        var securityKey = new SymmetricSecurityKey(new byte[256 / 8]);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "Demo App",
            Expires = DateTime.UtcNow.AddHours(2),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}