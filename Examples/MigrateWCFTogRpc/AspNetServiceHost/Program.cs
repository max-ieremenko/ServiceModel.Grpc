using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace AspNetServiceHost;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        PersonModule.ConfigureServices(builder.Services);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(SharedConfiguration.AspNetgRPCPersonServicePort, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();

        app.UseRouting();

        // host PersonService
        app.MapGrpcService<PersonService>();

        return app.RunAsync();
    }
}