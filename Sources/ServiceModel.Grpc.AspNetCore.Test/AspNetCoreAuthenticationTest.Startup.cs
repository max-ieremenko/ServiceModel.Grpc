using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ServiceModel.Grpc.AspNetCore
{
    public partial class AspNetCoreAuthenticationTest
    {
        private sealed class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices(IServiceCollection services)
            {
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
                            ValidateIssuerSigningKey = false,
                            ValidateTokenReplay = false,
                            RequireAudience = false,
                            RequireExpirationTime = false,
                            IssuerSigningKey = new SymmetricSecurityKey(Guid.Empty.ToByteArray())
                        };
                    });

                services.AddServiceModelGrpc();
                services.AddControllers();

                services.AddHttpContextAccessor();
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseRouting();

                app
                    .UseAuthentication()
                    .UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapGrpcService<Service>();
                });
            }
        }
    }
}
