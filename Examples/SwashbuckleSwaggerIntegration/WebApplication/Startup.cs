using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebApplication.Services;

namespace WebApplication
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // enable ServiceModel.Grpc
            services.AddServiceModelGrpc();

            // enable ServiceModel.Grpc integration for Swashbuckle.AspNetCore
            services.AddServiceModelGrpcSwagger();

            // required by Swashbuckle.AspNetCore
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.EnableAnnotations(true, true);
                c.UseAllOfForInheritance();
                c.SchemaGeneratorOptions.SubTypesSelector = SwaggerTools.GetDataContractKnownTypes;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                // host FigureService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
                endpoints.MapGrpcService<FigureService>();
            });
        }
    }
}
