using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Server;

public static class Program
{
    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            await ClientCalls.CallCalculator(new Uri("http://localhost:8080"), CancellationToken.None);
            await host.StopAsync();
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
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

        app.MapGrpcService<Calculator>();

        await app.StartAsync();
        return app;
    }
}