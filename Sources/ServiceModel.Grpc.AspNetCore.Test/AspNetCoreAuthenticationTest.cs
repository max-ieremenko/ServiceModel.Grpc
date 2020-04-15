using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class AspNetCoreAuthenticationTest
    {
        private KestrelHost _host;
        private IService _domainService;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = new KestrelHost();

            await _host.StartAsync(
                services =>
                {
                    services.AddHttpContextAccessor();
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
                                ValidateIssuerSigningKey = false,
                                ValidateTokenReplay = false,
                                RequireAudience = false,
                                RequireExpirationTime = false,
                                IssuerSigningKey = new SymmetricSecurityKey(Guid.Empty.ToByteArray())
                            };
                        });
                },
                app =>
                {
                    app
                        .UseAuthentication()
                        .UseAuthorization();
                },
                endpoints => endpoints.MapGrpcService<Service>());

            _domainService = _host.ClientFactory.CreateClient<IService>(_host.Channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync();
        }

        [Test]
        public void TryAccessWithoutAuthentication()
        {
            var ex = Assert.Throws<RpcException>(() => _domainService.GetCurrentUserName());

            var status = ex.Trailers.FirstOrDefault(i => i.Key.Equals(":status", StringComparison.OrdinalIgnoreCase))?.Value;
            status.ShouldBe("401");
        }

        [Test]
        public void GetCurrentUserName()
        {
            var headers = CreateMetadataWithToken("user-name");

            var name = _domainService.GetCurrentUserName(new CallOptions(headers));

            name.ShouldBe("user-name");
        }

        [Test]
        public void TryGetCurrentUserNameWithoutAuthentication()
        {
            var name = _domainService.TryGetCurrentUserName();

            name.ShouldBeNullOrEmpty();
        }

        [Test]
        public void TryGetCurrentUserNameWithAuthentication()
        {
            var headers = CreateMetadataWithToken("user-name");

            var name = _domainService.TryGetCurrentUserName(new CallOptions(headers));

            name.ShouldBe("user-name");
        }

        private static Metadata CreateMetadataWithToken(string userName)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userName)
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Guid.Empty.ToByteArray()), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            var tokenString = handler.WriteToken(token);

            return new Metadata
            {
                { "Authorization", "Bearer " + tokenString }
            };
        }
    }
}
