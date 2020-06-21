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
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class NativeServiceCompatibilityTest
    {
        private KestrelHost _grpcHost = null!;
        private KestrelHost _serviceModelHost = null!;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _grpcHost = new KestrelHost(ProtobufMarshallerFactory.Default);

            await _grpcHost.StartAsync(configureEndpoints: endpoints => endpoints.MapGrpcService<GreeterService>());

            _serviceModelHost = new KestrelHost(ProtobufMarshallerFactory.Default, 8081);

            await _serviceModelHost.StartAsync(configureEndpoints: endpoints => endpoints.MapGrpcService<DomainGreeterService>());
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _grpcHost.DisposeAsync();
            await _serviceModelHost.DisposeAsync();
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task HelloNativeCall(string channelName)
        {
            var client = new Greeter.GreeterClient(GetChannel(channelName));
            var response = await client.HelloAsync(new HelloRequest { Name = "world" });

            response.Message.ShouldBe("Hello world!");
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task HelloAllNativeCall(string channelName)
        {
            var client = new Greeter.GreeterClient(GetChannel(channelName));
            var response = new List<string>();

            using (var call = client.HelloAll(GreeterService.CreateHelloAllHeader("Hello")))
            {
                foreach (var name in new[] { "person 1", "person 2" })
                {
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
                }

                await call.RequestStream.CompleteAsync();

                while (await call.ResponseStream.MoveNext(default))
                {
                    response.Add(call.ResponseStream.Current.Message);
                }
            }

            response.ShouldBe(new[] { "Hello person 1!", "Hello person 2!" });
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task HelloDomainCall(string channelName)
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(GetChannel(channelName));
            var response = await client.HelloAsync("world");

            response.ShouldBe("Hello world!");
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task HelloAllDomainCall(string channelName)
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(GetChannel(channelName));
            var response = await client.HelloAllAsync(new[] { "person 1", "person 2" }.AsAsyncEnumerable(), "Hello").ToListAsync();

            response.ShouldBe(new[] { "Hello person 1!", "Hello person 2!" });
        }

        private ChannelBase GetChannel(string name)
        {
            if (name == "Native")
            {
                return _grpcHost.Channel;
            }

            if (name == "Domain")
            {
                return _serviceModelHost.Channel;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
