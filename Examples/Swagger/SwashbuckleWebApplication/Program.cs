using System;
using Contract;
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
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "1.0" });
            options.EnableAnnotations(true, true);
            options.UseAllOfForInheritance();
            options.SchemaGeneratorOptions.SubTypesSelector = SwaggerTools.GetDataContractKnownTypes;

            options.IncludeXmlComments(typeof(ICalculator).Assembly);
            options.IncludeXmlComments(typeof(Calculator).Assembly, includeControllerXmlComments: true);
        });

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        // enable ServiceModel.Grpc integration for Swashbuckle.AspNetCore
        builder.Services.AddServiceModelGrpcSwagger(options =>
        {
            // add method type into operation summary and method signature into description
            options.AutogenerateOperationSummaryAndDescription = true;
        });

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