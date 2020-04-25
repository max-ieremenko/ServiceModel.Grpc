using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;
using Unity;

namespace AspNetServiceHost
{
    internal sealed class Startup
    {
        public void ConfigureContainer(IUnityContainer container)
        {
            // configure container
            PersonModule.ConfigureContainer(container);
        }

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
                // host PersonService
                endpoints.MapGrpcService<PersonService>();
            });
        }
    }
}
