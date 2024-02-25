using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Client.Blazor.Services;
using SimpleChat.Client.Shared;

namespace SimpleChat.Client.Blazor;

public static class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        ConfigureServices(builder.Services);

        return builder.Build().RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorizationCore();

        services.AddSingleton<AuthenticationStateProvider, AuthenticationProvider>();
        services.AddTransient(provider => (IAuthenticationProvider)provider.GetRequiredService<AuthenticationStateProvider>());

        services.AddChatHttp11Client(provider =>
        {
            var baseAddress = provider.GetRequiredService<IWebAssemblyHostEnvironment>().BaseAddress;
            return new Uri(baseAddress);
        });
    }
}