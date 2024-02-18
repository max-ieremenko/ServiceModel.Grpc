using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;

namespace ServerAspNetHost;

public static class Program
{
    public static Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services
            .AddServiceModelGrpc(options =>
            {
                // set ProtobufMarshaller as default Marshaller
                options.DefaultMarshallerFactory = ProtobufMarshallerFactory.Default;
            });

        builder.WebHost
            .ConfigureKestrel(options => options.ListenLocalhost(ServiceConfiguration.ServiceModelGrpcPort, l => l.Protocols = HttpProtocols.Http2));

        var app = builder.Build();
        app.UseRouting();

        app.MapGrpcService<CalculatorService>();

        return app.RunAsync();
    }
}