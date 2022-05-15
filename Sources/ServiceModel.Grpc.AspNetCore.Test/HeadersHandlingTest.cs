// <copyright>
// Copyright 2020-2022 Max Ieremenko
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

using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture(GrpcChannelType.GrpcCore)]
    [TestFixture(GrpcChannelType.GrpcDotNet)]
    public class HeadersHandlingTest : HeadersHandlingTestBase
    {
        private readonly GrpcChannelType _channelType;
        private KestrelHost _host = null!;

        public HeadersHandlingTest(GrpcChannelType channelType)
        {
            _channelType = channelType;
        }

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = await new KestrelHost()
                .ConfigureClientFactory(options => options.DefaultCallOptionsFactory = () => new CallOptions(DefaultMetadata))
                .ConfigureEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<HeadersService>();
                })
                .WithChannelType(_channelType)
                .StartAsync()
                .ConfigureAwait(false);

            DomainService = _host.ClientFactory.CreateClient<IHeadersService>(_host.Channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync().ConfigureAwait(false);
        }
    }
}
