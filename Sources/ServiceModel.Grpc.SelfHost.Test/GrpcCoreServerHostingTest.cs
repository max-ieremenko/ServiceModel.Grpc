using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.TestApi;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;
using GrpcChannel = Grpc.Core.Channel;

namespace ServiceModel.Grpc.SelfHost
{
    [TestFixture]
    public class GrpcCoreServerHostingTest
    {
        private const int Port = 8080;
        private GrpcChannel _channel;
        private Server _server;
        private IMultipurposeService _domainService;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            _server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", Port, ServerCredentials.Insecure)
                }
            };

            _channel = new GrpcChannel("localhost", Port, ChannelCredentials.Insecure);

            _server.Services.AddServiceModelSingleton(new MultipurposeService());

            _domainService = new ClientFactory().CreateClient<IMultipurposeService>(_channel);

            _server.Start();
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _channel.ShutdownAsync();
            await _server.ShutdownAsync();
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
