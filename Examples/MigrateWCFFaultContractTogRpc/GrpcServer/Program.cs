using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace GrpcServer;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // configure container
        DebugModule.ConfigureServices(builder.Services);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc(options =>
        {
            // register server error handler
            options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
        });

        var app = builder.Build();

        app.UseRouting();

        // host DebugService
        app.MapGrpcService<DebugService>();

        return app.RunAsync();
    }
}