using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace ServerAspNetCore
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => o.ListenLocalhost(ServiceConfiguration.Port, l => l.Protocols = HttpProtocols.Http2));
                })
                .Build()
                .RunAsync();
        }
    }
}
