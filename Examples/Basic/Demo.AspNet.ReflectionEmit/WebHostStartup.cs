using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace Demo.AspNet.ReflectionEmit
{
    internal sealed class WebHostStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // enable ServiceModel.Grpc
            services.AddServiceModelGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // host PersonService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
                endpoints.MapGrpcService<PersonService>();
            });
        }
    }
}
