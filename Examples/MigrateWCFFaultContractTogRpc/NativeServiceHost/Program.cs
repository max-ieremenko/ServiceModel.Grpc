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
                DebugModule.ConfigureServices(services);

                services.AddHostedService<ServerHost>();
            })
            .Build()
            .RunAsync();
    }
}