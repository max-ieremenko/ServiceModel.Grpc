using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Unity.Microsoft.DependencyInjection;

namespace AspNetServiceHost
{
    public static class Program
    {
        public static Task Main()
        {
            return Host
                .CreateDefaultBuilder()
                .UseUnityServiceProvider()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.UseKestrel(o => o.ListenLocalhost(
                        SharedConfiguration.AspNetgRPCPersonServicePort,
                        l => l.Protocols = HttpProtocols.Http2));

                })
                .Build()
                .RunAsync();
        }
    }
}
