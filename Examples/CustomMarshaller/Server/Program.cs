using System;
using System.Threading.Tasks;
using CustomMarshaller;
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

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                // set JsonMarshallerFactory as default Marshaller
                options.DefaultMarshallerFactory = JsonMarshallerFactory.Default;
            });

        var app = builder.Build();

        app.UseRouting();
        app.MapGrpcService<PersonService>();

        return app.RunAsync();
    }
}