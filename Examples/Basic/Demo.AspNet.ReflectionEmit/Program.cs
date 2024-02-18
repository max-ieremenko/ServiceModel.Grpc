using System.Threading.Tasks;
using Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

namespace Demo.AspNet.ReflectionEmit;

public static class Program
{
    private const int Port = 8080;

    public static async Task Main()
    {
        using (var host = await StartWebHost())
        {
            var clientCalls = new ClientCalls();

            // a proxy for IPersonService will be generated at runtime by ServiceModel.Grpc
            await clientCalls.CallPersonService(Port);

            await host.StopAsync();
        }
    }

    private static async Task<IHost> StartWebHost()
    {
        var builder = WebApplication.CreateBuilder();

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();

        app.UseRouting();

        // host PersonService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<PersonService>();

        await app.StartAsync();
        return app;
    }
}