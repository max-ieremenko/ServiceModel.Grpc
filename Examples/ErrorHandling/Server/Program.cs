using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Services;
using ServiceModel.Grpc.Interceptors;

namespace Server;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

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

                // uncomment to fully control ServerFaultDetail.Detail serialization, must be uncommented in Client as well
                //options.DefaultErrorDetailSerializer = new CustomServerFaultDetailSerializer();
            });

        var app = builder.Build();
        
        app.UseRouting();
        app.MapGrpcService<DebugService>();

        return app.RunAsync();
    }
}