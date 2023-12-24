using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace WebApplication.Services;

internal sealed class DemoService : IDemoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DemoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [AllowAnonymous]
    public Task<string> PingAsync()
    {
        var userName = GetHttpContextUserName();

        return Task.FromResult("pong " + userName);
    }

    // see Program.cs: RequireAuthenticatedUser by default
    public Task<string> GetCurrentUserNameAsync()
    {
        var userName = GetHttpContextUserName();

        return Task.FromResult(userName);
    }

    private string GetHttpContextUserName()
    {
        var identity = _httpContextAccessor.HttpContext!.User.Identity;
        return identity?.IsAuthenticated == true ? identity.Name : "<unauthorized>";
    }
}