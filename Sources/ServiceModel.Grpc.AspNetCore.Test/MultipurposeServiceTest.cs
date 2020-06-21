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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public class MultipurposeServiceTest : MultipurposeServiceTestBase
    {
        private KestrelHost _host = null!;
        private Greeter.GreeterClient _greeterService = null!;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = new KestrelHost();

            await _host.StartAsync(configureEndpoints: endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<MultipurposeService>();
            });

            DomainService = _host.ClientFactory.CreateClient<IMultipurposeService>(_host.Channel);

            _greeterService = new Greeter.GreeterClient(_host.Channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync();
        }

        [Test]
        public async Task NativeGreet()
        {
            var actual = await _greeterService.HelloAsync(new HelloRequest { Name = "world" });

            actual.Message.ShouldBe("Hello world!");
        }
    }
}
