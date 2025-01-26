using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace GrpcServer;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        PersonModule.ConfigureServices(builder.Services);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        var app = builder.Build();

        app.UseRouting();

        // host PersonService
        app.MapGrpcService<PersonService>();

        return app.RunAsync();
    }
}