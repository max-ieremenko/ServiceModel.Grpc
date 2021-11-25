using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;
using Service.Filters;

namespace ServerAspNetHost
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
            services.AddTransient<LoggingServerFilter>();

            // attach LoggingServerFilter globally
            services.AddServiceModelGrpc(options =>
            {
                options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());
            });

            // attach LoggingServerFilter only for Calculator service
            ////services.AddServiceModelGrpcServiceOptions<Calculator>(options =>
            ////{
            ////    options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());
            ////});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Calculator>();
            });
        }
    }
}
