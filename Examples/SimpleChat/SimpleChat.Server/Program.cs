using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using SimpleChat.Server.Services;
using SimpleChat.Server.Shared;

namespace SimpleChat.Server;

public static class Program
{
    private const string GrpcCorsPolicy = "AllowGrpc";

    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        ConfigureAuthentication(builder.Services);
        ConfigureGrpc(builder.Services);
        ConfigureRazor(builder.Services);
        ConfigureApp(builder.Services);

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
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.UseGrpcWeb();
        app.UseCors();

        app.MapGrpcService<AccountService>().EnableGrpcWeb().RequireCors(GrpcCorsPolicy);
        app.MapGrpcService<ChatService>().EnableGrpcWeb().RequireCors(GrpcCorsPolicy);

        app.Run();
    }

    private static void ConfigureApp(IServiceCollection services)
    {
        // required by ChatService
        services.AddHttpContextAccessor();

        services.AddChatServer();
    }

    private static void ConfigureRazor(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([MediaTypeNames.Application.Octet]);
        });

        services.AddControllersWithViews();
        services.AddRazorPages();
    }

    private static void ConfigureGrpc(IServiceCollection services)
    {
        services.AddServiceModelGrpc();

        // required if the domain in the browser url differs from server hosted domain
        // for more details see https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-7.0#grpc-web-and-cors
        services.AddCors(options => options.AddPolicy(GrpcCorsPolicy, builder =>
        {
            builder
                .AllowAnyOrigin()
                .WithMethods(HttpMethods.Post)
                .AllowAnyHeader()
                .WithExposedHeaders(HeaderNames.GrpcStatus, HeaderNames.GrpcMessage, HeaderNames.GrpcEncoding, HeaderNames.GrpcAcceptEncoding);
        }));
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthorization();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateActor = false,
                    IssuerSigningKey = AccountService.GetDummyKey()
                };
            });
    }
}