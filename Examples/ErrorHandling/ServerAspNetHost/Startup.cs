using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace ServerAspNetHost
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServerErrorHandler>(_ =>
            {
                // combine application and unexpected handlers into one handler
                var collection = new ServerErrorHandlerCollection(
                    new ApplicationExceptionServerHandler(),
                    new UnexpectedExceptionServerHandler());

                return collection;
            });

            services
                .AddServiceModelGrpc(options =>
                {
                    options.DefaultErrorHandlerFactory = serviceProvider => serviceProvider.GetRequiredService<IServerErrorHandler>();
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DebugService>();
            });
        }
    }
}
