/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Server/Startup.cs
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;

namespace ServerAspNetHost;

internal sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // register ServerLoggerInterceptor globally (for all hosted services)
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ServerLoggerInterceptor>();
        });

        // register ServerLoggerInterceptor only for GreeterService
        ////services
        ////    .AddGrpc()
        ////    .AddServiceOptions<GreeterService>(options =>
        ////    {
        ////        options.Interceptors.Add<ServerLoggerInterceptor>();
        ////    });

        services.AddServiceModelGrpc();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<GreeterService>();
        });
    }
}