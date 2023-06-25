using System.Linq;
using BlazorApp.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace BlazorApp.Server;

internal sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddServiceModelGrpc();

        services.AddResponseCompression(options =>
        {
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
        });

        // required if the domain in the browser url differs from server hosted domain
        // for more details see https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-7.0#grpc-web-and-cors
        services.AddCors(options => options.AddPolicy("AllowGrpc", builder =>
        {
            builder
                .AllowAnyOrigin()
                .WithMethods(HttpMethods.Post)
                .AllowAnyHeader()
                .WithExposedHeaders(HeaderNames.GrpcStatus, HeaderNames.GrpcMessage, HeaderNames.GrpcEncoding, HeaderNames.GrpcAcceptEncoding);
        }));

        services.AddControllersWithViews();
        services.AddRazorPages();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseGrpcWeb();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapFallbackToFile("index.html");
            endpoints.MapGrpcService<WeatherForecastService>().EnableGrpcWeb().RequireCors("AllowGrpc");
        });
    }
}