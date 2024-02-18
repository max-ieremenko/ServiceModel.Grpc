using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;
using ServiceModel.Grpc.Configuration;

namespace Demo.ServerAspNetCore;

public static class Program
{
    private const int Port = 8081;

    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            await ClientCalls.CallPersonService(Port);

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                // set MessagePackMarshaller as default Marshaller
                options.DefaultMarshallerFactory = MessagePackMarshallerFactory.Default;
            });

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();

        app.UseRouting();

        app.MapGrpcService<PersonService>();

        await app.StartAsync();
        return app;
    }
}