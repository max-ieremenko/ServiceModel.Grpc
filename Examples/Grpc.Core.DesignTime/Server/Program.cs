using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Services;

namespace Server;

public static class Program
{
    public static Task Main(string[] args) =>
        Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddTransient<Calculator>();
                services.AddTransient<ServerLoggerInterceptor>();

                services.AddHostedService<ServerHost>();
            })
            .Build()
            .RunAsync();
}