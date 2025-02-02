using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Services;

namespace Server;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddServiceModelGrpc();

        var app = builder.Build();

        app.UseRouting();

        // register generated GenericCalculatorInt32Endpoint, see MyGrpcServices
        app.MapGenericCalculatorInt32();

        // endpoint will be generated on-fly
        app.MapGrpcService<DoubleCalculator>();

        return app.RunAsync();
    }
}