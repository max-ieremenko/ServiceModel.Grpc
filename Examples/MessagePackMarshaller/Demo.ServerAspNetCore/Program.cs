using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Demo.ServerAspNetCore;

public static class Program
{
    private const int Port = 8081;

    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            await ClientCalls.CallPersonService(Port);

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel(o => o.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));

            })
            .Build();

        await host.StartAsync();
        return host;
    }
}