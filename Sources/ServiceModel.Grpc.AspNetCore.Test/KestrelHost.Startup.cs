// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.AspNetCore
{
    internal partial class KestrelHost
    {
        private static IMarshallerFactory _marshallerFactory;
        private static Action<IServiceCollection> _configureServices;
        private static Action<IApplicationBuilder> _configureApp;
        private static Action<IEndpointRouteBuilder> _configureEndpoints;

        private sealed class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddGrpc();

                services
                    .AddServiceModelGrpc(options =>
                    {
                        options.DefaultMarshallerFactory = _marshallerFactory;
                    });

                services.AddTransient<TestMiddleware>();

                _configureServices?.Invoke(services);
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<TestMiddleware>();
                app.UseRouting();

                _configureApp?.Invoke(app);

                if (_configureEndpoints != null)
                {
                    app.UseEndpoints(_configureEndpoints);
                }
            }
        }

        private sealed class TestMiddleware : IMiddleware
        {
            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
