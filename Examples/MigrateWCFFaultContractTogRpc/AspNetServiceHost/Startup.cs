using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Interceptors;

namespace AspNetServiceHost;

internal sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // configure container
        DebugModule.ConfigureServices(services);

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