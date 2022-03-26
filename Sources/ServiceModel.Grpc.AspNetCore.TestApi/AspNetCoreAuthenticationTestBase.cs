// <copyright>
// Copyright 2020-2021 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
using ServiceModel.Grpc.AspNetCore.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore.TestApi
{
    public abstract class AspNetCoreAuthenticationTestBase
    {
        private KestrelHost _host = null!;
        private IServiceWithAuthentication _domainService = null!;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = new KestrelHost()
                .ConfigureServices(services =>
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
                })
                .ConfigureApp(app =>
                {
                    app
                        .UseAuthentication()
                        .UseAuthorization();
                });

            ConfigureKestrelHost(_host);
            await _host.StartAsync().ConfigureAwait(false);

            _domainService = _host.ClientFactory.CreateClient<IServiceWithAuthentication>(_host.Channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync().ConfigureAwait(false);
        }

        [Test]
        public void TryAccessWithoutAuthentication()
        {
            var ex = Assert.Throws<RpcException>(() => _domainService.GetCurrentUserName());

            ex.ShouldNotBeNull();
            ex.StatusCode.ShouldBe(StatusCode.Unauthenticated);
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

        protected abstract void ConfigureKestrelHost(KestrelHost host);

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
