using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;

namespace Server.CodeFirst;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                // set ProtobufMarshaller as default Marshaller
                options.DefaultMarshallerFactory = ProtobufMarshallerFactory.Default;
            });

        var app = builder.Build();
        app.UseRouting();

        app.MapGrpcService<CalculatorService>();

        return app.RunAsync();
    }
}