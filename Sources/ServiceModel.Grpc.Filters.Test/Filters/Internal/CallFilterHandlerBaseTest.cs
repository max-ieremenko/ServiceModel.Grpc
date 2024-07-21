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

using NUnit.Framework;

namespace ServiceModel.Grpc.Filters.Internal;

[TestFixture]
public partial class CallFilterHandlerBaseTest
{
    private IContext _context = null!;
    private Mock<IFilter> _filter = null!;
    private Mock<IFilter> _filter2 = null!;
    private CallFilterHandler _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _context = new Mock<IContext>(MockBehavior.Strict).Object;
        _filter = new Mock<IFilter>(MockBehavior.Strict);
        _filter2 = new Mock<IFilter>(MockBehavior.Strict);
        _sut = new CallFilterHandler(_context, [_filter.Object, _filter2.Object]);
    }

    [Test]
    public async Task InvokeAsync()
    {
        var stack = new List<string>();

        Func<IContext, ValueTask> last = async context =>
        {
            context.ShouldBe(_context);
            await Task.Yield();
            stack.Add("last");
        };

        _filter
            .Setup(f => f.InvokeAsync(_context, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IContext, Func<ValueTask>>((_, next) =>
            {
                stack.Add("filter1");
                return next();
            });

        _filter2
            .Setup(f => f.InvokeAsync(_context, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IContext, Func<ValueTask>>((_, next) =>
            {
                stack.Add("filter2");
                return next();
            });

        await _sut.InvokeAsync(last).ConfigureAwait(false);

        stack.ShouldBe(new[] { "filter1", "filter2", "last" });
    }

    [Test]
    public async Task InvokeAsyncSkipLast()
    {
        var stack = new List<string>();

        Func<IContext, ValueTask> last = _ => throw new NotSupportedException();

        _filter
            .Setup(f => f.InvokeAsync(_context, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IContext, Func<ValueTask>>((_, _) =>
            {
                stack.Add("filter");
                return new ValueTask(Task.CompletedTask);
            });

        await _sut.InvokeAsync(last).ConfigureAwait(false);

        stack.ShouldBe(new[] { "filter" });
    }

    [Test]
    public async Task InvokeAsyncTryNextTwoTimes()
    {
        var stack = new List<string>();
        var callsCounter = new int[3];

        Func<IContext, ValueTask> last = _ =>
        {
            Interlocked.Increment(ref callsCounter[2]);
            stack.Add("last");
            return new ValueTask(Task.CompletedTask);
        };

        _filter
            .Setup(f => f.InvokeAsync(_context, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IContext, Func<ValueTask>>(async (_, next) =>
            {
                Interlocked.Increment(ref callsCounter[0]);
                stack.Add("filter1");
                await next().ConfigureAwait(false);
                await next().ConfigureAwait(false);
            });

        _filter2
            .Setup(f => f.InvokeAsync(_context, It.IsNotNull<Func<ValueTask>>()))
            .Returns<IContext, Func<ValueTask>>(async (_, next) =>
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

    [Test]
    public void InvokeSync()
    {
        var stack = new List<string>();

        Action<IContext> last = context =>
        {
            context.ShouldBe(_context);
            stack.Add("last");
        };

        _filter
            .Setup(f => f.Invoke(_context, It.IsNotNull<Action>()))
            .Callback<IContext, Action>((_, next) =>
            {
                stack.Add("filter1");
                next();
            });

        _filter2
            .Setup(f => f.Invoke(_context, It.IsNotNull<Action>()))
            .Callback<IContext, Action>((_, next) =>
            {
                stack.Add("filter2");
                next();
            });

        _sut.Invoke(last);

        stack.ShouldBe(new[] { "filter1", "filter2", "last" });
    }

    [Test]
    public void InvokeSyncSkipLast()
    {
        var stack = new List<string>();

        Action<IContext> last = _ => throw new NotSupportedException();

        _filter
            .Setup(f => f.Invoke(_context, It.IsNotNull<Action>()))
            .Callback<IContext, Action>((_, _) =>
            {
                stack.Add("filter");
            });

        _sut.Invoke(last);

        stack.ShouldBe(new[] { "filter" });
    }

    [Test]
    public void InvokeSyncTryNextTwoTimes()
    {
        var stack = new List<string>();
        var callsCounter = new int[3];

        Action<IContext> last = _ =>
        {
            Interlocked.Increment(ref callsCounter[2]);
            stack.Add("last");
        };

        _filter
            .Setup(f => f.Invoke(_context, It.IsNotNull<Action>()))
            .Callback<IContext, Action>((_, next) =>
            {
                Interlocked.Increment(ref callsCounter[0]);
                stack.Add("filter1");
                next();
                next();
            });

        _filter2
            .Setup(f => f.Invoke(_context, It.IsNotNull<Action>()))
            .Callback<IContext, Action>((_, next) =>
            {
                Interlocked.Increment(ref callsCounter[1]);
                stack.Add("filter2");
                next();
                next();
            });

        _sut.Invoke(last);

        stack.ShouldBe(new[] { "filter1", "filter2", "last" });
        callsCounter.ShouldBe(new[] { 1, 1, 1 });
    }
}