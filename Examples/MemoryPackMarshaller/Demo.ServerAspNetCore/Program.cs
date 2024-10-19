using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceModel.Grpc.Configuration;

namespace Demo.ServerAspNetCore;

public static class Program
{
    private const int Port = 8081;

    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            await ClientCalls.RunAsync(Port);

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                // set MemoryPackMarshaller as default Marshaller
                options.DefaultMarshallerFactory = MemoryPackMarshallerFactory.Default;
            });

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();

        app.UseRouting();

        GrpcServices.MapAllGrpcServices(app);

        await app.StartAsync();
        return app;
    }
}