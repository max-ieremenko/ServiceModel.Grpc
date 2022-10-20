using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServerSelfHost
{
    public static class Program
    {
        public static Task Main()
        {
            return Host
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ServerHostedService>();
                })
                .Build()
                .RunAsync();
        }
    }
}
