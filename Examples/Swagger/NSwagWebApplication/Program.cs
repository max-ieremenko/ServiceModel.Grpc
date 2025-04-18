using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwagWebApplication.Services;

namespace NSwagWebApplication;

public static class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // enable detailed errors in gRPC response
        builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);

        // NSwag.AspNetCore
        builder.Services.AddMvc();
        builder.Services.AddOpenApiDocument(settings =>
        {
            settings.DocumentName = "v1";
            settings.Title = "My API";
            settings.Version = "1.0";
            settings.UseControllerSummaryAsTagDescription = true;
        });

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        // enable ServiceModel.Grpc integration for NSwag.AspNetCore
        builder.Services.AddServiceModelGrpcSwagger(options =>
        {
            // add method type into operation summary and method signature into description
            options.AutogenerateOperationSummaryAndDescription = true;
        });

        var app = builder.Build();
        app.UseRouting();

        // NSwag.AspNetCore
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseReDoc();

        // Enable ServiceModel.Grpc HTTP/1.1 JSON gateway for Swagger UI, button "Try it out"
        app.UseServiceModelGrpcSwaggerGateway();

        // host FigureService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<FigureService>();

        // host Calculator, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<Calculator>();

        app.Run();
    }
}