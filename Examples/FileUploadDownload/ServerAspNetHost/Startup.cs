using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Mime;
using Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Service;

namespace ServerAspNetHost
{
    internal sealed class Startup
    {
        public Startup(bool useResponseCompression)
        {
            UseResponseCompression = useResponseCompression;
        }
        
        public bool UseResponseCompression { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (UseResponseCompression)
            {
                // enable response compression for gRPC
                services
                    .AddGrpc()
                    .AddServiceOptions<FileService>(options =>
                    {
                        options.ResponseCompressionLevel = CompressionSettings.Level;
                        options.ResponseCompressionAlgorithm = CompressionSettings.Algorithm;
                    })
                    .AddServiceOptions<FileServiceRentedArray>(options =>
                    {
                        options.ResponseCompressionLevel = CompressionSettings.Level;
                        options.ResponseCompressionAlgorithm = CompressionSettings.Algorithm;
                    });

                // enable response compression for asp.net
                services.AddResponseCompression(options =>
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
                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionSettings.Level;
                });
            }

            services
                .AddServiceModelGrpc()
                .AddServiceModelGrpcServiceOptions<FileService>(options => options.MarshallerFactory = DemoMarshallerFactory.Default)
                .AddServiceModelGrpcServiceOptions<FileServiceRentedArray>(options => options.MarshallerFactory = DemoMarshallerFactory.Default);

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (UseResponseCompression)
            {
                app.UseResponseCompression();
            }

            app.UseRouting();
            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGrpcService<FileService>().EnableGrpcWeb();
                endpoints.MapGrpcService<FileServiceRentedArray>().EnableGrpcWeb();
            });
        }
    }
}
