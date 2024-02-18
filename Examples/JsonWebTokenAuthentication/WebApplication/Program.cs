using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebApplication.Services;

namespace WebApplication;

public static class Program
{
    public static Task Main()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        // enable ServiceModel.Grpc
        builder.Services.AddServiceModelGrpc();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // WARN: insecure connection should be used only in development environments
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateActor = false,

                    // dummy key, only for demo purposes, must be synchronized with Client
                    IssuerSigningKey = new SymmetricSecurityKey(new byte[256 / 8])
                };
            });

        var app = builder.Build();

        app
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization();

        // host DemoService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        app.MapGrpcService<DemoService>();

        return app.RunAsync();
    }
}