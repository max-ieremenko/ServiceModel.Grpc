using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication;

public static class Program
{
    public static Task Main()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        var app = builder.Build();

        app
            .UseRouting();

        // host Calculator and RandomNumberGenerator, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<Calculator>();
        app.MapGrpcService<RandomNumberGenerator>();

        return app.RunAsync();
    }
}