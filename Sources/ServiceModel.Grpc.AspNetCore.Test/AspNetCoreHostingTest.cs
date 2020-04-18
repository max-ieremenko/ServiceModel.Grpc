using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public class AspNetCoreHostingTest
    {
        private KestrelHost _host;
        private IMultipurposeService _domainService;
        private Greeter.GreeterClient _greeterService;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _host = new KestrelHost();

            await _host.StartAsync(configureEndpoints: endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<MultipurposeService>();
            });

            _domainService = _host.ClientFactory.CreateClient<IMultipurposeService>(_host.Channel);

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
        public async Task Sum5ValuesAsync()
        {
            var actual = await _domainService.Sum5ValuesAsync(1, 2, 3, 4, 5, default);

            actual.ShouldBe(15);
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
        public async Task MultiplyByAndSumValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await _domainService.MultiplyByAndSumValues(values, 2);

            actual.ShouldBe(12);
        }

        [Test]
        public async Task ConvertValues()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await _domainService.ConvertValues(values).ToListAsync();

            actual.ShouldBe(new[] { "1", "2", "3" });
        }

        [Test]
        public async Task MultiplyBy()
        {
            var values = new[] { 1, 2, 3 }.AsAsyncEnumerable();

            var actual = await _domainService.MultiplyBy(values, 2).ToListAsync();

            actual.ShouldBe(new[] { 2, 4, 6 });
        }
    }
}
