using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WcfServer;

public static class Program
{
    public static Task Main(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<WcfHost>();
            })
            .Build()
            .RunAsync();
    }
}