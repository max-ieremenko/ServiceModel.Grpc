using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

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
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddServiceModelGrpc();

        var app = builder.Build();

        app.UseRouting();

        app.MapGrpcService<PersonService>();

        await app.StartAsync();
        return app;
    }
}