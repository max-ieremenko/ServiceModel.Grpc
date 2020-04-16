using System;
using System.Collections.Generic;
using System.Linq;
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

            actual.Headers.ShouldBe(newOptions.CallOptions.Value.Headers);
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
        
        [Test]
        [TestCaseSource(nameof(GetMergeMetadataTestCases))]
        public void MergeMetadata(Metadata current, Metadata mergeWith, Metadata expected)
        {
            var actual = CallOptionsBuilder.MergeMetadata(current, mergeWith);

            actual.Select(i => i.Key).ShouldBe(expected.Select(i => i.Key), Case.Sensitive);
        }

        private static IEnumerable<TestCaseData> GetMergeMetadataTestCases()
        {
            var current = new Metadata();
            var mergeWith = new Metadata();

            yield return new TestCaseData(null, mergeWith, mergeWith);
            yield return new TestCaseData(current, null, current);

            yield return new TestCaseData(
                new Metadata
                {
                    { "x", "1" }
                },
                new Metadata(),
                new Metadata
                {
                    { "x", "1" }
                });

            yield return new TestCaseData(
                new Metadata(),
                new Metadata
                {
                    { "x", "1" }
                },
                new Metadata
                {
                    { "x", "1" }
                });

            yield return new TestCaseData(
                new Metadata
                {
                    { "x", "1" }
                },
                new Metadata
                {
                    { "x", "1" }
                },
                new Metadata
                {
                    { "x", "1" }
                });

            yield return new TestCaseData(
                new Metadata
                {
                    { "x", "1" }
                },
                new Metadata
                {
                    { "x", "2" }
                },
                new Metadata
                {
                    { "x", "1" },
                    { "x", "2" }
                });

            yield return new TestCaseData(
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 2 } }
                },
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 2 } }
                },
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 2 } }
                });

            yield return new TestCaseData(
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 2 } }
                },
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 3 } }
                },
                new Metadata
                {
                    { "x-bin", new byte[] { 1, 2 } },
                    { "x-bin", new byte[] { 1, 3 } }
                });
        }
    }
}
