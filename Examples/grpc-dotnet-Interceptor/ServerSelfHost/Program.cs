using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

namespace ServerSelfHost;

public static class Program
{
    public static Task Main(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddTransient<GreeterService>();
                services.AddTransient<ServerLoggerInterceptor>();

                services.AddHostedService(serviceProvider => new ServerHost(serviceProvider));
            })
            .Build()
            .RunAsync();
    }
}