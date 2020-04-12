using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore
{
    public partial class AspNetCoreHostingTest
    {
        private sealed class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddControllers();

                services.AddGrpc(options =>
                {
                    options.ResponseCompressionLevel = CompressionLevel.Optimal;
                });

                services
                    .AddServiceModelGrpc(options =>
                    {
                        options.DefaultMarshallerFactory = new DataContractMarshallerFactory();
                    })
                    .AddServiceModelGrpcServiceOptions<MultipurposeService>(options =>
                    {
                        options.MarshallerFactory = null;
                    });

                services.AddTransient<TestMiddleware>();
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseMiddleware<TestMiddleware>();
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapGrpcService<GreeterService>();
                    endpoints.MapGrpcService<MultipurposeService>();
                });
            }
        }

        private sealed class TestMiddleware : IMiddleware
        {
            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
