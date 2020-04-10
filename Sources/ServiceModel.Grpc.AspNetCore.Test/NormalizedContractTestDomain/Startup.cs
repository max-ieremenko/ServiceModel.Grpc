using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.AspNetCore.NormalizedContractTestDomain
{
    internal sealed class Startup
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
                endpoints.MapGrpcService<MultipurposeService>();
            });
        }
    }
}
