// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi;

public abstract class HeadersHandlingTestBase
{
    protected Metadata DefaultMetadata { get; } = new Metadata
    {
        { HeadersService.DefaultHeaderName, HeadersService.DefaultHeaderValue }
    };

    protected IHeadersService DomainService { get; set; } = null!;

    [Test]
    public void UnaryCall()
    {
        var context = CreateCallContext();

        DomainService.UnaryCall(context);

        // see CallInvoker.BlockingUnaryCall: no API to access response headers
        context.ResponseHeaders.ShouldBeNull();
        context.ResponseTrailers.ShouldBeNull();
        context.ResponseStatus.ShouldBeNull();
    }

    [Test]
    public async Task UnaryCallAsync()
    {
        var context = CreateCallContext();

        await DomainService.UnaryCallAsync(context).ConfigureAwait(false);

        ValidateResponse(context);
    }

    [Test]
    public async Task ServerStreamingCall()
    {
        var context = CreateCallContext();

        var stream = DomainService.ServerStreamingCall(context);

        // in ServerStreamingCallAsync headers are already available
        context.ResponseHeaders.ShouldBeNull();
        context.ResponseTrailers.ShouldBeNull();

        var values = new List<int>();
        await foreach (var i in stream.ConfigureAwait(false))
        {
            context.ResponseHeaders.ShouldNotBeNull();

            if (values.Count == 0)
            {
                // InvalidOperationException : Trailers can only be accessed once the call has finished.
                Assert.Throws<InvalidOperationException>(() => _ = context.ResponseTrailers);
            }

            values.Add(i);
        }

        ValidateResponse(context);
        values.Count.ShouldBe(10);
    }

    [Test]
    public async Task ServerStreamingCallAsync()
    {
        var context = CreateCallContext();

        var stream = await DomainService.ServerStreamingCallAsync(context).ConfigureAwait(false);

        // in ServerStreamingCall headers are not available yet
        ValidateResponse(context, true);

        var values = new List<int>();
        await foreach (var i in stream.ConfigureAwait(false))
        {
            if (values.Count == 0)
            {
                // InvalidOperationException : Trailers can only be accessed once the call has finished.
                Assert.Throws<InvalidOperationException>(() => _ = context.ResponseTrailers);
            }

            values.Add(i);
        }

        ValidateResponse(context);
        values.Count.ShouldBe(10);
    }

    [Test]
    public async Task ClientStreamingCall()
    {
        var context = CreateCallContext();

        await DomainService.ClientStreamingCall(Enumerable.Range(1, 10).AsAsyncEnumerable(), context).ConfigureAwait(false);

        ValidateResponse(context);
    }

    [Test]
    public async Task DuplexStreamingCall()
    {
        var context = CreateCallContext();

        var stream = DomainService.DuplexStreamingCall(Enumerable.Range(1, 10).AsAsyncEnumerable(), context);

        // in DuplexStreamingCallAsync headers are already available
        context.ResponseHeaders.ShouldBeNull();
        context.ResponseTrailers.ShouldBeNull();

        var values = new List<int>();
        await foreach (var i in stream.ConfigureAwait(false))
        {
            context.ResponseHeaders.ShouldNotBeNull();

            if (values.Count == 0)
            {
                // InvalidOperationException : Trailers can only be accessed once the call has finished.
                Assert.Throws<InvalidOperationException>(() => _ = context.ResponseTrailers);
            }

            values.Add(i);
        }

        ValidateResponse(context);
        values.Count.ShouldBe(10);
    }

    [Test]
    public async Task DuplexStreamingCallAsync()
    {
        var context = CreateCallContext();

        var stream = await DomainService.DuplexStreamingCallAsync(Enumerable.Range(1, 10).AsAsyncEnumerable(), context).ConfigureAwait(false);

        // in DuplexStreamingCall headers are not available yet
        ValidateResponse(context, true);

        var values = new List<int>();
        await foreach (var i in stream.ConfigureAwait(false))
        {
            if (values.Count == 0)
            {
                // InvalidOperationException : Trailers can only be accessed once the call has finished.
                Assert.Throws<InvalidOperationException>(() => _ = context.ResponseTrailers);
            }

            values.Add(i);
        }

        ValidateResponse(context);
        values.Count.ShouldBe(10);
    }

    private static void ValidateResponse(CallContext context, bool headersOnly = false)
    {
        context.ResponseHeaders.ShouldNotBeNull();

        var defaultHeader = context.ResponseHeaders.FindHeader(HeadersService.DefaultHeaderName, false);
        defaultHeader.ShouldNotBeNull();
        defaultHeader.Value.ShouldBe(HeadersService.DefaultHeaderValue);

        var callHeader = context.ResponseHeaders.FindHeader(HeadersService.CallHeaderName, false);
        callHeader.ShouldNotBeNull();
        callHeader.Value.ShouldBe(HeadersService.CallHeaderValue);

        if (headersOnly)
        {
            // InvalidOperationException : Trailers can only be accessed once the call has finished.
            Assert.Throws<InvalidOperationException>(() => _ = context.ResponseTrailers);
        }
        else
        {
            context.ResponseTrailers.ShouldNotBeNull();

            var callTrailer = context.ResponseTrailers.FindHeader(HeadersService.CallTrailerName, false);
            callTrailer.ShouldNotBeNull();
            callTrailer.Value.ShouldBe(HeadersService.CallTrailerValue);
        }
    }

    private static CallContext CreateCallContext()
    {
        return new CallContext(new Metadata
        {
            { HeadersService.CallHeaderName, HeadersService.CallHeaderValue }
        });
    }
}