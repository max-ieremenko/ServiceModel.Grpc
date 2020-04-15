using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class NativeServiceCompatibilityTest
    {
        private KestrelHost _grpcHost;
        private KestrelHost _serviceModelHost;

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
        public async Task NativeNativeCall()
        {
            var client = new Greeter.GreeterClient(_grpcHost.Channel);
            var response = await client.HelloAsync(new HelloRequest { Name = "world" });

            response.Message.ShouldBe("Hello world!");
        }

        [Test]
        public async Task DomainDomainCall()
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(_serviceModelHost.Channel);
            var response = await client.HelloAsync("world");

            response.ShouldBe("Hello world!");
        }

        [Test]
        public async Task NativeDomainCall()
        {
            var client = new Greeter.GreeterClient(_serviceModelHost.Channel);
            var response = await client.HelloAsync(new HelloRequest { Name = "world" });

            response.Message.ShouldBe("Hello world!");
        }

        [Test]
        public async Task DomainNativeCall()
        {
            var client = _serviceModelHost.ClientFactory.CreateClient<IDomainGreeterService>(_grpcHost.Channel);
            var response = await client.HelloAsync("world");

            response.ShouldBe("Hello world!");
        }
    }
}
