using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Service;
using ServiceModel.Grpc.Configuration;

namespace Demo.ServerAspNetCore
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddServiceModelGrpc(options =>
                {
                    // set MessagePackMarshaller as default Marshaller
                    options.DefaultMarshallerFactory = MessagePackMarshallerFactory.Default;
                });
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
