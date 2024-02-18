using System;
using System.Linq;
using BlazorApp.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace BlazorApp.Server;

public static class Program
{
    private const string GrpcCorsPolicy = "AllowGrpc";

    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        builder.Services.AddServiceModelGrpc();

        builder.Services.AddResponseCompression(options =>
        {
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
        });

        // required if the domain in the browser url differs from server hosted domain
        // for more details see https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-7.0#grpc-web-and-cors
        builder.Services.AddCors(options => options.AddPolicy(GrpcCorsPolicy, policyOptions =>
        {
            policyOptions
                .AllowAnyOrigin()
                .WithMethods(HttpMethods.Post)
                .AllowAnyHeader()
                .WithExposedHeaders(HeaderNames.GrpcStatus, HeaderNames.GrpcMessage, HeaderNames.GrpcEncoding, HeaderNames.GrpcAcceptEncoding);
        }));

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        app.UseResponseCompression();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.UseGrpcWeb();
        app.UseCors();

        app.MapGrpcService<WeatherForecastService>().EnableGrpcWeb().RequireCors(GrpcCorsPolicy);

        app.Run();
    }
}