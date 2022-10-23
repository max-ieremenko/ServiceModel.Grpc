using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

namespace NativeServiceHost;

public static class Program
{
    public static Task Main(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                PersonModule.ConfigureServices(services);

                services.AddHostedService(serviceProvider => new ServerHost(serviceProvider));
            })
            .Build()
            .RunAsync();
    }
}