using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Demo.ServerAspNetCore;

public static class Program
{
    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            var calls = new ClientCalls(5000);

            calls.RunSync();
            await calls.RunAsync();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .Build();

        await host.StartAsync();
        return host;
    }
}