// <copyright>
// Copyright Max Ieremenko
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
using System.Linq;
using System.Threading;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Client.Internal;

[TestFixture]
public class CallOptionsBuilderTest
{
    private readonly CancellationTokenSource _tokenSource = new();

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
    public void WithNullCancellationToken()
    {
        var actual = new CallOptionsBuilder(null)
            .WithCancellationToken(null)
            .Build();

        actual.CancellationToken.ShouldBe(default);
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
    public void WithCallContext()
    {
        var defaultOptions = new CallOptions(new Metadata());
        var newOptions = new CallContext(new CallOptions(new Metadata()));

        var actual = new CallOptionsBuilder(() => defaultOptions)
            .WithCallContext(newOptions)
            .Build();

        actual.Headers.ShouldBe(newOptions.CallOptions!.Value.Headers);
    }

    [Test]
    public void WithNullCallOptions()
    {
        var options = new CallOptions(new Metadata());

        var actual = new CallOptionsBuilder(() => options)
            .WithCallOptions(null)
            .Build();

        actual.Headers.ShouldBe(options.Headers);
    }

    [Test]
    public void WithCallOptions()
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
    public void MergeMetadata(Metadata? current, Metadata? mergeWith, Metadata? expected)
    {
        var actual = CallOptionsBuilder.MergeMetadata(current, mergeWith);

        var actualKeys = actual?.Select(i => i.Key) ?? Enumerable.Empty<string>();
        var expectedKeys = expected?.Select(i => i.Key) ?? Enumerable.Empty<string>();
        actualKeys.ShouldBe(expectedKeys, Case.Sensitive);
    }

    private static IEnumerable<TestCaseData> GetMergeMetadataTestCases()
    {
        yield return new TestCaseData(null, new Metadata(), new Metadata());
        yield return new TestCaseData(new Metadata(), null, null);

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