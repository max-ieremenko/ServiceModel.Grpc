using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Server;

public static class Program
{
    public static async Task Main()
    {
        using (var host = await StartWebHost().ConfigureAwait(false))
        {
            await ClientCalls.CallCalculator(new Uri("http://localhost:8080"), CancellationToken.None).ConfigureAwait(false);
            await host.StopAsync().ConfigureAwait(false);
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder => builder.SetBasePath(AppContext.BaseDirectory))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .Build();

        await host.StartAsync().ConfigureAwait(false);
        return host;
    }
}