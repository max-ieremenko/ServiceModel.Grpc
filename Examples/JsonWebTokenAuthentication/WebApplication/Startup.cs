using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using WebApplication.Services;

namespace WebApplication
{
    internal sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // enable ServiceModel.Grpc
            services.AddServiceModelGrpc();

            services.AddHttpContextAccessor();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services
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
                        IssuerSigningKey = new SymmetricSecurityKey(Guid.Empty.ToByteArray())
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app
                .UseAuthentication()
                .UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // host DemoService, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
                endpoints.MapGrpcService<DemoService>();
            });
        }
    }
}
