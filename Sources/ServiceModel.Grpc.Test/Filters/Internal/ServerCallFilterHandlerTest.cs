// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public class ServerCallFilterHandlerTest
{
    private Mock<IServerFilterContextInternal> _context = null!;
    private Mock<IServerFilter> _filter = null!;
    private Mock<IServerFilter> _filter2 = null!;
    private ServerCallFilterHandler _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _context = new Mock<IServerFilterContextInternal>(MockBehavior.Strict);
        _filter = new Mock<IServerFilter>(MockBehavior.Strict);
        _filter2 = new Mock<IServerFilter>(MockBehavior.Strict);
        _sut = new ServerCallFilterHandler(_context.Object, new[] { _filter.Object, _filter2.Object });
    }

    [Test]
    public async Task Invoke()
    {
        var stack = new List<string>();

        Func<IServerFilterContextInternal, ValueTask> last = async context =>
        {
            context.ShouldBe(_context.Object);
            await Task.Yield();
            stack.Add("last");
        };

        _filter
            .Setup(f => f.InvokeAsync(_context.Object, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IServerFilterContextInternal, Func<ValueTask>>((context, next) =>
            {
                stack.Add("filter1");
                return next();
            });

        _filter2
            .Setup(f => f.InvokeAsync(_context.Object, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IServerFilterContextInternal, Func<ValueTask>>((context, next) =>
            {
                stack.Add("filter2");
                return next();
            });

        await _sut.InvokeAsync(last).ConfigureAwait(false);

        stack.ShouldBe(new[] { "filter1", "filter2", "last" });
    }

    [Test]
    public async Task InvokeSkipLast()
    {
        var stack = new List<string>();

        Func<IServerFilterContextInternal, ValueTask> last = context => throw new NotSupportedException();

        _filter
            .Setup(f => f.InvokeAsync(_context.Object, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IServerFilterContextInternal, Func<ValueTask>>((context, next) =>
            {
                stack.Add("filter");
                return new ValueTask(Task.CompletedTask);
            });

        await _sut.InvokeAsync(last).ConfigureAwait(false);

        stack.ShouldBe(new[] { "filter" });
    }

    [Test]
    public async Task FilterTryNextTwoTimes()
    {
        var stack = new List<string>();
        var callsCounter = new int[3];

        Func<IServerFilterContextInternal, ValueTask> last = context =>
        {
            Interlocked.Increment(ref callsCounter[2]);
            stack.Add("last");
            return new ValueTask(Task.CompletedTask);
        };

        _filter
            .Setup(f => f.InvokeAsync(_context.Object, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IServerFilterContextInternal, Func<ValueTask>>(async (context, next) =>
            {
                Interlocked.Increment(ref callsCounter[0]);
                stack.Add("filter1");
                await next().ConfigureAwait(false);
                await next().ConfigureAwait(false);
            });

        _filter2
            .Setup(f => f.InvokeAsync(_context.Object, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IServerFilterContextInternal, Func<ValueTask>>(async (context, next) =>
            {
                Interlocked.Increment(ref callsCounter[1]);
                stack.Add("filter2");
                await next().ConfigureAwait(false);
                await next().ConfigureAwait(false);
            });

        await _sut.InvokeAsync(last).ConfigureAwait(false);

        stack.ShouldBe(new[] { "filter1", "filter2", "last" });
        callsCounter.ShouldBe(new[] { 1, 1, 1 });
    }
}