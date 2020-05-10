using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;
using Unity;

namespace AspNetServiceHost
{
    internal sealed class Startup
    {
        public void ConfigureContainer(IUnityContainer container)
        {
            // configure container
            DebugModule.ConfigureContainer(container);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // enable ServiceModel.Grpc
            services.AddServiceModelGrpc(options =>
            {
                // register server error handler
                options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // host DebugService
                endpoints.MapGrpcService<DebugService>();
            });
        }
    }
}
