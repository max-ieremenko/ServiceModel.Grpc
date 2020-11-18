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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.AspNetCore.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public class AspNetCoreAuthenticationTest : AspNetCoreAuthenticationTestBase
    {
        protected override void ConfigureKestrelHost(KestrelHost host)
        {
            host
                .ConfigureServices(services => services.AddTransient<IServiceWithAuthentication, ServiceWithAuthentication>())
                .ConfigureEndpoints(endpoints => endpoints.MapGrpcService<IServiceWithAuthentication>());
        }
    }
}
