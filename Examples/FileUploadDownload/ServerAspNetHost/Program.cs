using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServerAspNetHost;

public static class Program
{
    public static void Main()
    {
        BuildHost(false).Run();
    }

    public static IHost BuildHost(bool useResponseCompression)
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup(_ => new Startup(useResponseCompression));
            })
            .Build();
    }
}