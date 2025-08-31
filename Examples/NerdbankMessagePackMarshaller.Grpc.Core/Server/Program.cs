using System.Threading.Tasks;
using Server.Services;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Interceptors;
using Microsoft.Extensions.Hosting;

namespace Server;

public static class Program
{
    public static Task Main(string[] args) =>
        Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(AddApplicationServices)
            .Build()
            .RunAsync();

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddHostedService<ServerHost>();

        services.AddTransient<Calculator>();
        services.AddSingleton<IServerErrorHandler>(_ => new ServerErrorHandlerCollection(new ServerErrorHandler()));
        services.AddTransient<LoggingServerFilter>();
    }
}