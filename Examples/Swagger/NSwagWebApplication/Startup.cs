using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSwagWebApplication.Services;

namespace NSwagWebApplication
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // enable detailed errors in gRPC response
            services.AddGrpc(options => options.EnableDetailedErrors = true);

            // NSwag.AspNetCore
            services.AddMvc();
            services.AddOpenApiDocument(settings =>
            {
                settings.DocumentName = "v1";
                settings.Title = "My API";
                settings.Version = "1.0";
            });

            // enable ServiceModel.Grpc
            services.AddServiceModelGrpc();

            // enable ServiceModel.Grpc integration for Swashbuckle.AspNetCore
            services.AddServiceModelGrpcSwagger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // NSwag.AspNetCore
            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseReDoc();

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
}
