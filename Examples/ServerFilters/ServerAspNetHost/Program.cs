using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServerAspNetHost
{
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
                .ConfigureAppConfiguration(builder => builder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false, false))
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build();

            await host.StartAsync().ConfigureAwait(false);
            return host;
        }
    }
}
