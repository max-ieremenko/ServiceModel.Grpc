using System;
using System.Threading;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class CallOptionsBuilderTest
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        [Test]
        public void BuildDefault()
        {
            var actual = new CallOptionsBuilder(null).Build();

            actual.CancellationToken.ShouldBe(default);
        }

        [Test]
        public void DefaultFromFactory()
        {
            var options = new CallOptions(new Metadata());

            var actual = new CallOptionsBuilder(() => options).Build();

            actual.Headers.ShouldBe(options.Headers);
        }

        [Test]
        public void WithCancellationToken()
        {
            var actual = new CallOptionsBuilder(null)
                .WithCancellationToken(_tokenSource.Token)
                .Build();

            actual.CancellationToken.ShouldBe(_tokenSource.Token);
        }

        [Test]
        public void WithNullServerCallContext()
        {
            var actual = new CallOptionsBuilder(null)
                .WithServerCallContext(null)
                .Build();

            actual.CancellationToken.ShouldBe(default);
        }

        [Test]
        public void WithServerCallContext()
        {
            var context = new Mock<ServerCallContext>(MockBehavior.Strict);

            Assert.Throws<NotSupportedException>(() => new CallOptionsBuilder(null).WithServerCallContext(context.Object));
        }

        [Test]
        public void IgnoreEmptyCallContext()
        {
            var options = new CallOptions(new Metadata());

            var actual = new CallOptionsBuilder(() => options)
                .WithCallContext(default)
                .Build();

            actual.Headers.ShouldBe(options.Headers);
        }
        
        [Test]
        public void AcceptCallContext()
        {
            var defaultOptions = new CallOptions(new Metadata());
            var newOptions = new CallContext(new CallOptions(new Metadata()));

            var actual = new CallOptionsBuilder(() => defaultOptions)
                .WithCallContext(newOptions)
                .Build();

            actual.Headers.ShouldBe(newOptions.ClientCallContext.Value.Headers);
        }

        [Test]
        public void AcceptCallOptions()
        {
            var defaultOptions = new CallOptions(new Metadata());
            var newOptions = new CallOptions(new Metadata());

            var actual = new CallOptionsBuilder(() => defaultOptions)
                .WithCallOptions(newOptions)
                .Build();

            actual.Headers.ShouldBe(newOptions.Headers);
        }
    }
}
