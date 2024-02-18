using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace AspNetServiceHost;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        // configure container
        DebugModule.ConfigureServices(builder.Services);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc(options =>
        {
            // register server error handler
            options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
        });

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(SharedConfiguration.AspNetgRPCDebugServicePort, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();

        app.UseRouting();

        // host DebugService
        app.MapGrpcService<DebugService>();

        return app.RunAsync();
    }
}