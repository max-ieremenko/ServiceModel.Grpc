using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SwashbuckleWebApplication.Configuration;
using SwashbuckleWebApplication.Services;

namespace SwashbuckleWebApplication;

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

        // Swashbuckle.AspNetCore
        builder.Services.AddMvc();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "1.0" });
            c.EnableAnnotations(true, true);
            c.UseAllOfForInheritance();
            c.SchemaGeneratorOptions.SubTypesSelector = SwaggerTools.GetDataContractKnownTypes;
        });

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        // enable ServiceModel.Grpc integration for Swashbuckle.AspNetCore
        builder.Services.AddServiceModelGrpcSwagger();

        var app = builder.Build();
        app.UseRouting();

        // Swashbuckle.AspNetCore
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "1.0");
        });

        // Enable ServiceModel.Grpc HTTP/1.1 JSON gateway for Swagger UI, button "Try it out"
        app.UseServiceModelGrpcSwaggerGateway();

        // host FigureService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<FigureService>();

        // host Calculator, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<Calculator>();

        app.Run();
    }
}