using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;

namespace ServerAspNetCore
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddServiceModelGrpc(options =>
                {
                    // set ProtobufMarshaller as default Marshaller
                    options.DefaultMarshallerFactory = ProtobufMarshallerFactory.Default;
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
