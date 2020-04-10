using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class AspNetCoreHostingTest
    {
        private const int Port = 8080;
        private IHost _host;
        private GrpcChannel _channel;
        private IMultipurposeService _domainService;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => o.ListenLocalhost(Port, l => l.Protocols = HttpProtocols.Http2));
                })
                .Build();

            await _host.StartAsync();

            GrpcChannelExtensions.Http2UnencryptedSupport = true;
            _channel = GrpcChannel.ForAddress($"http://localhost:{Port}");
            _domainService = new ClientFactory().CreateClient<IMultipurposeService>(_channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            _channel?.Dispose();
            await _host.StopAsync();
        }

        [Test]
        public void ConcatB()
        {
            var context = new CallOptions().WithHeaders(new Metadata
            {
                { "value", "b" }
            });

            var actual = _domainService.Concat("a", context);

            actual.ShouldBe("ab");
        }

        [Test]
        public async Task ConcatBAsync()
        {
            var context = new CallOptions().WithHeaders(new Metadata
            {
                { "value", "b" }
            });

            var actual = await _domainService.ConcatAsync("a", context);

            actual.ShouldBe("ab");
        }

        [Test]
        public async Task RepeatValue()
        {
            var actual = await _domainService.RepeatValue("a", 3).ToListAsync();

            actual.ShouldBe(new[] { "a", "a", "a" });
        }

        [Test]
        public async Task SumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await _domainService.SumValues(values);

            actual.ShouldBe(6);
        }

        [Test]
        public async Task ConvertValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await _domainService.ConvertValues(values).ToListAsync();

            actual.ShouldBe(new[] { "1", "2", "3" });
        }
    }
}
