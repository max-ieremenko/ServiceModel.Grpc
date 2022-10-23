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
        var user = _httpContextAccessor.HttpContext!.User;

        return Task.FromResult("pong " + user.Identity?.Name);
    }

    // see Startup.cs: RequireAuthenticatedUser by default
    public Task<string> GetCurrentUserNameAsync()
    {
        var user = _httpContextAccessor.HttpContext!.User;

        return Task.FromResult(user.Identity!.Name);
    }
}