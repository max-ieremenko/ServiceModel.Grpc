using Microsoft.AspNetCore.Builder;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Server.Services;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Interceptors;
using Contract;
using Microsoft.AspNetCore.Routing;

namespace Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        AddApplicationServices(builder.Services);

        await using var app = builder.Build();

        MapApplicationServices(app);

        await app.RunAsync();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        services.AddServiceModelGrpc(options =>
        {
            // set NerdbankMessagePackMarshaller with generated formatters as default Marshaller
            options.DefaultMarshallerFactory = NerdbankMessagePackMarshallerFactory.CreateWithTypeShapeProviderFrom<Point>();

            // Filters: log gRPC calls
            options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());

            // Error handling: activate ServerErrorHandler
            options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();

            // Error handling: AOT compatible marshalling of InvalidRectangleError
            options.DefaultErrorDetailSerializer = new ServerFaultDetailSerializer();
        });

        services.AddSingleton<IServerErrorHandler>(_ => new ServerErrorHandlerCollection(new ServerErrorHandler()));

        services.AddTransient<LoggingServerFilter>();
    }

    private static void MapApplicationServices(IEndpointRouteBuilder builder)
    {
        GrpcServices.MapAllGrpcServices(builder);
    }
}