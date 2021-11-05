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
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.AspNetCore.TestApi;
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
            _grpcHost = await new KestrelHost()
                .ConfigureClientFactory(options => options.MarshallerFactory = ProtobufMarshallerFactory.Default)
                .ConfigureEndpoints(endpoints => endpoints.MapGrpcService<GreeterService>())
                .StartAsync()
                .ConfigureAwait(false);

            _serviceModelHost = await new KestrelHost(8081)
                .ConfigureClientFactory(options => options.MarshallerFactory = ProtobufMarshallerFactory.Default)
                .ConfigureEndpoints(endpoints => endpoints.MapGrpcService<DomainGreeterService>())
                .StartAsync()
                .ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _grpcHost.DisposeAsync().ConfigureAwait(false);
            await _serviceModelHost.DisposeAsync().ConfigureAwait(false);
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task UnaryNativeCall(string channelName)
        {
            var client = new Greeter.GreeterClient(GetChannel(channelName));
            var response = await client.UnaryAsync(new HelloRequest { Name = "world" }).ResponseAsync.ConfigureAwait(false);

            response.Message.ShouldBe("Hello world!");
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task DuplexStreamingNativeCall(string channelName)
        {
            var client = new Greeter.GreeterClient(GetChannel(channelName));
            var response = new List<string>();

            using (var call = client.DuplexStreaming(CompatibilityToolsTestExtensions.SerializeMethodInput(ProtobufMarshallerFactory.Default, "Hello")))
            {
                var responseHeaders = await call.ResponseHeadersAsync.ConfigureAwait(false);
                CompatibilityToolsTestExtensions.DeserializeMethodOutput<string>(ProtobufMarshallerFactory.Default, responseHeaders).ShouldBe("Hello");

                foreach (var name in new[] { "person 1", "person 2" })
                {
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name }).ConfigureAwait(false);
                }

                await call.RequestStream.CompleteAsync().ConfigureAwait(false);

                while (await call.ResponseStream.MoveNext(default).ConfigureAwait(false))
                {
                    response.Add(call.ResponseStream.Current.Message);
                }
            }

            response.ShouldBe(new[] { "Hello person 1!", "Hello person 2!" });
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task UnaryDomainCall(string channelName)
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(GetChannel(channelName));
            var response = await client.UnaryAsync("world").ConfigureAwait(false);

            response.ShouldBe("Hello world!");
        }

        [Test]
        [TestCase("Native")]
        [TestCase("Domain")]
        public async Task DuplexStreamingDomainCall(string channelName)
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(GetChannel(channelName));
            var (greet, stream) = await client.DuplexStreaming(new[] { "person 1", "person 2" }.AsAsyncEnumerable(), "Hello").ConfigureAwait(false);

            greet.ShouldBe("Hello");
            var response = await stream.ToListAsync().ConfigureAwait(false);
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
