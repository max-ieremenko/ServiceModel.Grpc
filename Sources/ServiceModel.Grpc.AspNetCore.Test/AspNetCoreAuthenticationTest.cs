// <copyright>
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

using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.AspNetCore.TestApi.Domain;
using ServiceModel.Grpc.TestApi;

namespace ServiceModel.Grpc.AspNetCore;

[TestFixture(GrpcChannelType.GrpcCore)]
[TestFixture(GrpcChannelType.GrpcDotNet)]
public class AspNetCoreAuthenticationTest : AspNetCoreAuthenticationTestBase
{
    private readonly GrpcChannelType _channelType;

    public AspNetCoreAuthenticationTest(GrpcChannelType channelType)
    {
        _channelType = channelType;
    }

    protected override void ConfigureKestrelHost(KestrelHost host)
    {
        host
            .WithChannelType(_channelType)
            .ConfigureServices(services => services.AddTransient<IServiceWithAuthentication, ServiceWithAuthentication>())
            .ConfigureEndpoints(endpoints => endpoints.MapGrpcService<IServiceWithAuthentication>());
    }
}