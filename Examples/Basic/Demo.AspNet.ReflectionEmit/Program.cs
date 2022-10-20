using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Demo.AspNet.ReflectionEmit;

public static class Program
{
    private const int Port = 8080;

    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            var clientCalls = new ClientCalls();

            // a proxy for IPersonService will be generated at runtime by ServiceModel.Grpc
            await clientCalls.CallPersonService(Port);

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<WebHostStartup>();

                webBuilder.UseKestrel(o => o.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));

            })
            .Build();

        await host.StartAsync();
        return host;
    }
}