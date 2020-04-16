using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.AspNetCore
{
    [TestFixture]
    public partial class HeadersHandlingTest
    {
        private KestrelHost _host;
        private IHeadersService _headersService;
        private Metadata _defaultMetadata;

        [OneTimeSetUp]
        public async Task BeforeAll()
        {
            _defaultMetadata = new Metadata
            {
                { "defaultHeader", "defaultHeader value" }
            };
            _host = new KestrelHost(defaultCallOptions: new CallOptions(_defaultMetadata));

            await _host.StartAsync(configureEndpoints: endpoints =>
            {
                endpoints.MapGrpcService<HeadersService>();
            });

            _headersService = _host.ClientFactory.CreateClient<IHeadersService>(_host.Channel);
        }

        [OneTimeTearDown]
        public async Task AfterAll()
        {
            await _host.DisposeAsync();
        }

        [Test]
        public void GetDefaultRequestHeader()
        {
            _headersService.GetRequestHeader(_defaultMetadata[0].Key).ShouldBe(_defaultMetadata[0].Value);
        }

        [Test]
        public void GetRequestHeader()
        {
            var options = new CallOptions(new Metadata
            {
                { "h1", "value 1" }
            });

            _headersService.GetRequestHeader("h1", options).ShouldBe("value 1");
        }

        [Test]
        public async Task WriteResponseHeader()
        {
            var context = new CallContext();

            await _headersService.WriteResponseHeader("h1", "value 1", context);

            var actual = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            actual.ShouldBe(new[] { "value 1" });
        }

        [Test]
        public async Task ServerStreamingWriteResponseHeader()
        {
            var context = new CallContext();

            var response = _headersService.ServerStreamingWriteResponseHeader("h1", "value 1", context);

            context.ResponseHeaders.ShouldBeNull();

            var values = new List<int>();
            await foreach (var i in response)
            {
                context.ResponseHeaders.ShouldNotBeNull();
                values.Add(i);
            }

            var actual = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            actual.ShouldBe(new[] { "value 1" });

            values.Count.ShouldBe(10);
        }

        [Test]
        public async Task ClientStreaming()
        {
            var context = new CallContext(new Metadata
            {
                { "h1", "value " }
            });

            var actual = await _headersService.ClientStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), context);

            context.ResponseHeaders.ShouldNotBeNull();
            actual.ShouldBe("value ");

            var header = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            header.ShouldBe(new[] { "value 2" });
        }

        [Test]
        public async Task DuplexStreaming()
        {
            var context = new CallContext(new Metadata
            {
                { "h1", "value " }
            });

            var response = _headersService.DuplexStreaming(new[] { 1, 2 }.AsAsyncEnumerable(), context);

            context.ResponseHeaders.ShouldBeNull();

            var values = new List<string>();
            await foreach (var i in response)
            {
                context.ResponseHeaders.ShouldNotBeNull();
                values.Add(i);
            }

            var header = context.ResponseHeaders.Where(i => i.Key == "h1").Select(i => i.Value).ToArray();
            header.ShouldBe(new[] { "value 2" });

            values.ShouldBe(new[] { "value " });
        }
    }
}
