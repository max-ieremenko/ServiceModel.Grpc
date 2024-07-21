﻿// <copyright>
// Copyright Max Ieremenko
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

using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.AspNetCore.TestApi.Domain;

namespace ServiceModel.Grpc.DesignTime.Generator.Test.AspNetCore;

[TestFixture]
[ExportGrpcService(typeof(IServiceWithAuthentication), GenerateAspNetExtensions = true)]
public partial class AspNetCoreAuthViaInterfaceTest : AspNetCoreAuthenticationTestBase
{
    protected override void ConfigureKestrelHost(KestrelHost host)
    {
        host
            .ConfigureServices(services => services.AddTransient<IServiceWithAuthentication, ServiceWithAuthentication>())
            .ConfigureEndpoints(endpoints => MapServiceWithAuthentication(endpoints));
    }

    // add attributes manually
    internal partial class ServiceWithAuthenticationEndpointBinder
    {
        partial void ServiceGetMetadataOverride(IList<object> metadata)
        {
            metadata.Add(new AuthorizeAttribute());
        }

        partial void MethodTryGetCurrentUserNameGetMetadataOverride(IList<object> metadata)
        {
            metadata.Add(new AllowAnonymousAttribute());
        }
    }
}