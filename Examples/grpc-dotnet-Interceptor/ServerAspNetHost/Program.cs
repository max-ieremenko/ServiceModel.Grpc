/*
 * this is adapted for ServiceModel.Grpc example from grpc-dotnet repository
 * see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Server/Program.cs
 */

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace ServerAspNetHost;

public static class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // register ServerLoggerInterceptor globally (for all hosted services)
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ServerLoggerInterceptor>();
        });

        // register ServerLoggerInterceptor only for GreeterService
        ////builder
        ////    .Services
        ////    .AddGrpc()
        ////    .AddServiceOptions<GreeterService>(options =>
        ////    {
        ////        options.Interceptors.Add<ServerLoggerInterceptor>();
        ////    });

        builder.Services.AddServiceModelGrpc();

        var app = builder.Build();

        app.UseRouting();

        app.MapGrpcService<GreeterService>();

        app.Run();
    }
}