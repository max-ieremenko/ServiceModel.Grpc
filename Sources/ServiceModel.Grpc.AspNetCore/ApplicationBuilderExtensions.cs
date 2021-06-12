// <copyright>
// Copyright 2021 Max Ieremenko
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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceModel.Grpc.AspNetCore.Internal.Swagger;

namespace ServiceModel.Grpc.AspNetCore
{
    internal static class ApplicationBuilderExtensions
    {
        public static void UseSwagger(IApplicationBuilder app)
        {
            var test = app.ApplicationServices.GetService<IApiDescriptionAdapter>();
            if (test == null)
            {
                throw new InvalidOperationException("Missing services.AddServiceModelGrpcSwagger() in Startup.cs");
            }

            app.UseMiddleware<SwaggerUiMiddleware>();
        }
    }
}
