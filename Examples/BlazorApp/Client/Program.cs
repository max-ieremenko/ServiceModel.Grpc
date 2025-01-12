using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using ServiceModel.Grpc.Client;

namespace BlazorApp.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        ConfigureServices(builder.Services);

        await builder.Build().RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // register GrpcChannel
        services.AddSingleton(provider =>
        {
            var baseAddress = provider.GetRequiredService<IWebAssemblyHostEnvironment>().BaseAddress;
            var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
            return GrpcChannel.ForAddress(baseAddress, new GrpcChannelOptions { HttpHandler = httpHandler });
        });

        // register ClientFactory
        services.AddSingleton<IClientFactory>(_ =>
        {
            var factory = new ClientFactory();

            GrpcClients.AddAllClients(factory);

            return factory;
        });

        // resolve IWeatherForecastService from ClientFactory
        services.AddScoped(provider =>
        {
            var channel = provider.GetRequiredService<GrpcChannel>();
            return provider.GetRequiredService<IClientFactory>().CreateClient<IWeatherForecastService>(channel);
        });
    }
}