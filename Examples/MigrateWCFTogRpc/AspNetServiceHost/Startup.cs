using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace AspNetServiceHost;

internal sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        PersonModule.ConfigureServices(services);

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