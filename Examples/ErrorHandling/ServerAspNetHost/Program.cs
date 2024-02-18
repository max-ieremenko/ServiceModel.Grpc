using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace ServerAspNetHost;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton<IServerErrorHandler>(_ =>
        {
            // combine application and unexpected handlers into one handler
            var collection = new ServerErrorHandlerCollection(
                new ApplicationExceptionServerHandler(),
                new UnexpectedExceptionServerHandler());

            return collection;
        });

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
            });

        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(ServiceConfiguration.ServiceModelGrpcPort, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();
        
        app.UseRouting();
        app.MapGrpcService<DebugService>();

        return app.RunAsync();
    }
}