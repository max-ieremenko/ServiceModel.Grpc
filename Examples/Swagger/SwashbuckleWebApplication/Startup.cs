using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SwashbuckleWebApplication.Configuration;
using SwashbuckleWebApplication.Services;

namespace SwashbuckleWebApplication;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // enable detailed errors in gRPC response
        services.AddGrpc(options => options.EnableDetailedErrors = true);

        // Swashbuckle.AspNetCore
        services.AddMvc();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "1.0" });
            c.EnableAnnotations(true, true);
            c.UseAllOfForInheritance();
            c.SchemaGeneratorOptions.SubTypesSelector = SwaggerTools.GetDataContractKnownTypes;
        });

        // enable ServiceModel.Grpc
        services.AddServiceModelGrpc();

        // enable ServiceModel.Grpc integration for Swashbuckle.AspNetCore
        services.AddServiceModelGrpcSwagger();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        // Swashbuckle.AspNetCore
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "1.0");
        });

        // Enable ServiceModel.Grpc HTTP/1.1 JSON gateway for Swagger UI, button "Try it out"
        app.UseServiceModelGrpcSwaggerGateway();

        app.UseEndpoints(endpoints =>
        {
            // host FigureService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
            endpoints.MapGrpcService<FigureService>();

            // host Calculator, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
            endpoints.MapGrpcService<Calculator>();
        });
    }
}