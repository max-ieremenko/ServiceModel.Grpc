using System;
using Contract;
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Services;

namespace Server;

public static class Program
{
    public static void Main()
    {
        BuildHost(false).Run();
    }

    public static IHost BuildHost(bool useResponseCompression)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        if (useResponseCompression)
        {
            // enable response compression for gRPC
            builder
                .Services
                .AddGrpc()
                .AddServiceOptions<FileService>(options =>
                {
                    options.ResponseCompressionLevel = CompressionSettings.Level;
                    options.ResponseCompressionAlgorithm = CompressionSettings.Algorithm;
                });

            // enable response compression for asp.net
            builder.Services.AddResponseCompression(options =>
            {
                options.Providers.Clear();
                options.Providers.Add<GzipCompressionProvider>();

                // see FileServiceController.DownloadAsync
                options.MimeTypes = new HashSet<string>(ResponseCompressionDefaults.MimeTypes, StringComparer.OrdinalIgnoreCase)
                {
                    MediaTypeNames.Application.Octet
                };
            });

            // the same as for gRPC
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionSettings.Level;
            });
        }

        builder
            .Services
            .AddServiceModelGrpc()
            .AddServiceModelGrpcServiceOptions<FileService>(options => options.MarshallerFactory = DemoMarshallerFactory.Default);

        builder
            .Services
            .AddControllers()
            .AddApplicationPart(typeof(Program).Assembly);

        var app = builder.Build();

        if (useResponseCompression)
        {
            app.UseResponseCompression();
        }

        app.MapControllers();
        app.MapGrpcService<FileService>();

        return app;
    }
}