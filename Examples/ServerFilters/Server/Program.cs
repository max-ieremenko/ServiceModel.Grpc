using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Services;
using Server.Services.Filters;

namespace Server;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddTransient<LoggingServerFilter>();

        // attach LoggingServerFilter globally
        builder.Services.AddServiceModelGrpc(options =>
        {
            options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());
        });

        // attach LoggingServerFilter only for Calculator service
        ////builder.Services.AddServiceModelGrpcServiceOptions<Calculator>(options =>
        ////{
        ////    options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());
        ////});

        var app = builder.Build();

        app.UseRouting();

        app.MapGrpcService<Calculator>();

        return app.RunAsync();
    }
}