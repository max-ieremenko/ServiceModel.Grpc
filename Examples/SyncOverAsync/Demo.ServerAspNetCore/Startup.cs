using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace Demo.ServerAspNetCore
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PersonService>();
            });
        }
    }
}
