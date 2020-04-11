using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using Shouldly;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class AspNetCoreAuthenticationTest
    {
        private const int Port = 8080;
        private IHost _host;
        private GrpcChannel _channel;
        private IService _domainService;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => o.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));
                })
                .Build();

            await _host.StartAsync();

            GrpcChannelExtensions.Http2UnencryptedSupport = true;
            _channel = new GrpcChannel("localhost", Port, ChannelCredentials.Insecure);
            _domainService = new ClientFactory().CreateClient<IService>(_channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _channel.ShutdownAsync();
            await _host.StopAsync();
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
