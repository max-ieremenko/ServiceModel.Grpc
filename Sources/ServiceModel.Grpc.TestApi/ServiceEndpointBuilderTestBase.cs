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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Emit;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi;

public abstract class ServiceEndpointBuilderTestBase
{
    private Mock<IContract> _service = null!;
    private CancellationTokenSource _tokenSource = null!;
    private Mock<ServerCallContext> _serverCallContext = null!;

    protected Type ChannelType { get; set; } = null!;

    protected object Channel { get; set; } = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _service = new Mock<IContract>(MockBehavior.Strict);
        _tokenSource = new CancellationTokenSource();

        _serverCallContext = new Mock<ServerCallContext>(MockBehavior.Strict);
        _serverCallContext
            .Protected()
            .SetupGet<CancellationToken>("CancellationTokenCore")
            .Returns(_tokenSource.Token);
        _serverCallContext
            .Protected()
            .SetupGet<Metadata>("RequestHeadersCore")
            .Returns(Metadata.Empty);
        _serverCallContext
            .Protected()
            .SetupGet<DateTime>("DeadlineCore")
            .Returns(DateTime.MinValue);
        _serverCallContext
            .Protected()
            .SetupGet<DateTime>("DeadlineCore")
            .Returns(DateTime.MinValue);
        _serverCallContext
            .Protected()
            .SetupGet<WriteOptions>("WriteOptionsCore")
            .Returns(WriteOptions.Default);
    }

    [Test]
    public async Task Empty()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.Empty))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.Empty());

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task EmptyAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.EmptyAsync))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.EmptyAsync())
            .Returns(Task.CompletedTask);

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task EmptyValueTaskAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.EmptyValueTaskAsync))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.EmptyValueTaskAsync())
            .Returns(new ValueTask(Task.CompletedTask));

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task EmptyContext()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.EmptyContext))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.EmptyContext(It.IsNotNull<CallContext>()))
            .Callback<CallContext>(c =>
            {
                c.ServerCallContext.ShouldBe(_serverCallContext.Object);
            });

        var actual = await call(_service.Object, new Message(), _serverCallContext.Object).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task EmptyTokenAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.EmptyTokenAsync))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.EmptyTokenAsync(_tokenSource.Token))
            .Returns(Task.CompletedTask);

        var actual = await call(_service.Object, new Message(), _serverCallContext.Object).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task ReturnString()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ReturnString))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ReturnString())
            .Returns("a");

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.Value1.ShouldBe("a");
        _service.VerifyAll();
    }

    [Test]
    public async Task ReturnStringAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ReturnStringAsync))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ReturnStringAsync(_serverCallContext.Object))
            .Returns(Task.FromResult("a"));

        var actual = await call(_service.Object, new Message(), _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("a");
        _service.VerifyAll();
    }

    [Test]
    public async Task ReturnValueTaskBoolAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ReturnValueTaskBoolAsync))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message<bool>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ReturnValueTaskBoolAsync())
            .Returns(new ValueTask<bool>(Task.FromResult(true)));

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.Value1.ShouldBeTrue();
        _service.VerifyAll();
    }

    [Test]
    public async Task OneParameterContext()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.OneParameterContext))
            .CreateDelegate<Func<IContract, Message<int>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _serverCallContext
            .Protected()
            .SetupGet<Metadata>("RequestHeadersCore")
            .Returns(new Metadata());
        _serverCallContext
            .Protected()
            .SetupGet<DateTime>("DeadlineCore")
            .Returns(DateTime.Now);
        _serverCallContext
            .Protected()
            .SetupGet<WriteOptions>("WriteOptionsCore")
            .Returns(WriteOptions.Default);

        _service
            .Setup(s => s.OneParameterContext(It.IsAny<CallOptions>(), 3))
            .Callback<CallOptions, int>((options, _) =>
            {
                options.CancellationToken.ShouldBe(_tokenSource.Token);
            });

        var actual = await call(_service.Object, new Message<int>(3), _serverCallContext.Object).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task OneParameterAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.OneParameterAsync))
            .CreateDelegate<Func<IContract, Message<double>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.OneParameterAsync(3.5))
            .Returns(Task.CompletedTask);

        var actual = await call(_service.Object, new Message<double>(3.5), null!).ConfigureAwait(false);

        actual.ShouldNotBeNull();
        _service.VerifyAll();
    }

    [Test]
    public async Task AddTwoValues()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.AddTwoValues))
            .CreateDelegate<Func<IContract, Message<int, double>, ServerCallContext, Task<Message<double>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.AddTwoValues(1, 3.5))
            .Returns(4.5);

        var actual = await call(_service.Object, new Message<int, double>(1, 3.5), null!).ConfigureAwait(false);

        actual.Value1.ShouldBe(4.5);
        _service.VerifyAll();
    }

    [Test]
    public async Task ConcatThreeValueAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ConcatThreeValueAsync))
            .CreateDelegate<Func<IContract, Message<int, string, long>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ConcatThreeValueAsync(1, "a", _tokenSource.Token, 3))
            .Returns(Task.FromResult("1a3"));

        var actual = await call(_service.Object, new Message<int, string, long>(1, "a", 3), _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("1a3");
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateUnary1()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateUnary1", typeof(IContract), typeof(Message), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.DuplicateUnary())
            .Returns("a");

        var actual = await call(_service.Object, new Message(), null!).ConfigureAwait(false);

        actual.Value1.ShouldBe("a");
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateUnary2()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateUnary2", typeof(IContract), typeof(Message<string>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message<string>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.DuplicateUnary("a"))
            .Returns("b");

        var actual = await call(_service.Object, new Message<string>("a"), null!).ConfigureAwait(false);

        actual.Value1.ShouldBe("b");
        _service.VerifyAll();
    }

    [Test]
    public async Task UnaryNullableCancellationToken()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.UnaryNullableCancellationToken))
            .CreateDelegate<Func<IContract, Message<TimeSpan>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.UnaryNullableCancellationToken(TimeSpan.FromSeconds(2), _tokenSource.Token))
            .Returns(Task.CompletedTask);

        await call(_service.Object, new Message<TimeSpan>(TimeSpan.FromSeconds(2)), _serverCallContext.Object).ConfigureAwait(false);

        _service.VerifyAll();
    }

    [Test]
    public async Task UnaryNullableCallOptions()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.UnaryNullableCallOptions))
            .CreateDelegate<Func<IContract, Message<TimeSpan>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.UnaryNullableCallOptions(TimeSpan.FromSeconds(2), It.IsAny<CallOptions?>()))
            .Callback<TimeSpan, CallOptions?>((_, op) =>
            {
                op.ShouldNotBeNull();
                op.Value.CancellationToken.ShouldBe(_tokenSource.Token);
            })
            .Returns(Task.CompletedTask);

        await call(_service.Object, new Message<TimeSpan>(TimeSpan.FromSeconds(2)), _serverCallContext.Object).ConfigureAwait(false);

        _service.VerifyAll();
    }

    [Test]
    public async Task EmptyServerStreaming()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.EmptyServerStreaming))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.EmptyServerStreaming())
            .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

        var response = await call(_service.Object, new Message(), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task ServerStreamingRepeatValue()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ServerStreamingRepeatValue))
            .CreateDelegate<Func<IContract, Message<int, int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ServerStreamingRepeatValue(1, 2, _tokenSource.Token))
            .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

        var response = await call(_service.Object, new Message<int, int>(1, 2), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task ServerStreamingRepeatValueAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ServerStreamingRepeatValueAsync))
            .CreateDelegate<Func<IContract, Message<int, int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ServerStreamingRepeatValueAsync(1, 2, _tokenSource.Token))
            .Returns(Task.FromResult(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

        var response = await call(_service.Object, new Message<int, int>(1, 2), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task ServerStreamingRepeatValueValueTaskAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ServerStreamingRepeatValueValueTaskAsync))
            .CreateDelegate<Func<IContract, Message<int, int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ServerStreamingRepeatValueValueTaskAsync(1, 2, _tokenSource.Token))
            .Returns(new ValueTask<IAsyncEnumerable<int>>(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

        var response = await call(_service.Object, new Message<int, int>(1, 2), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task ServerStreamingWithHeadersTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ServerStreamingWithHeadersTask))
            .CreateDelegate<Func<IContract, Message<int, int>, ServerCallContext, ValueTask<(Message<int, int>?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ServerStreamingWithHeadersTask(1, 2, _tokenSource.Token))
            .Returns(Task.FromResult((1, new[] { 1, 2, 3 }.AsAsyncEnumerable(), 2)));

        var response = await call(_service.Object, new Message<int, int>(1, 2), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldNotBeNull();
        response.Item1.Value1.ShouldBe(1);
        response.Item1.Value2.ShouldBe(2);
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task ServerStreamingWithHeadersValueTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ServerStreamingWithHeadersValueTask))
            .CreateDelegate<Func<IContract, Message<int, int>, ServerCallContext, ValueTask<(Message<int>?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.ServerStreamingWithHeadersValueTask(1, 2, _tokenSource.Token))
            .Returns(new ValueTask<(IAsyncEnumerable<int> Stream, int Count)>((new[] { 1, 2, 3 }.AsAsyncEnumerable(), 2)));

        var response = await call(_service.Object, new Message<int, int>(1, 2), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldNotBeNull();
        response.Item1.Value1.ShouldBe(2);
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { 1, 2, 3 });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateServerStreaming1()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateServerStreaming1", typeof(IContract), typeof(Message), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.DuplicateServerStreaming())
            .Returns(new[] { "a" }.AsAsyncEnumerable());

        var response = await call(_service.Object, new Message(), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { "a" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateServerStreaming2()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateServerStreaming2", typeof(IContract), typeof(Message<string>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message<string>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        _service
            .Setup(s => s.DuplicateServerStreaming("b"))
            .Returns(new[] { "a" }.AsAsyncEnumerable());

        var response = await call(_service.Object, new Message<string>("b"), _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var actual = await response.Item2.ToListAsync().ConfigureAwait(false);
        actual.ShouldBe(new[] { "a" });
        _service.VerifyAll();
    }

    [Test]
    public async Task ClientStreamingEmpty()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ClientStreamingEmpty))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 2 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.ClientStreamingEmpty(It.IsNotNull<IAsyncEnumerable<int>>()))
            .Returns<IAsyncEnumerable<int>>(async values =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 2 });
            });

        await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        _service.VerifyAll();
    }

    [Test]
    public async Task ClientStreamingEmptyValueTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ClientStreamingEmptyValueTask))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 2 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.ClientStreamingEmptyValueTask(It.IsNotNull<IAsyncEnumerable<int>>()))
            .Returns<IAsyncEnumerable<int>>(async values =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 2 });
            });

        await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        _service.VerifyAll();
    }

    [Test]
    public async Task ClientStreamingSumValues()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ClientStreamingSumValues))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 2 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.ClientStreamingSumValues(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 2 });
                return "2";
            });

        var actual = await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("2");
        _service.VerifyAll();
    }

    [Test]
    public async Task ClientStreamingSumValuesValueTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ClientStreamingSumValuesValueTask))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 2 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.ClientStreamingSumValuesValueTask(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 2 });
                return "2";
            });

        var actual = await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("2");
        _service.VerifyAll();
    }

    [Test]
    public async Task ClientStreamingHeaderParameters()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.ClientStreamingHeaderParameters))
            .CreateDelegate<Func<IContract, Message<int, string>, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 2 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.ClientStreamingHeaderParameters(It.IsNotNull<IAsyncEnumerable<int>>(), 1, "prefix"))
            .Returns<IAsyncEnumerable<int>, int, string>(async (values, m, p) =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 2 });
                return "2";
            });

        var actual = await call(_service.Object, new Message<int, string>(1, "prefix"), stream, _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("2");
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateClientStreaming1()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateClientStreaming1", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<string>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<string>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { "a" }.AsAsyncEnumerable();

        _service
            .Setup(s => s.DuplicateClientStreaming(It.IsNotNull<IAsyncEnumerable<string>>()))
            .Returns<IAsyncEnumerable<string>>(async values =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { "a" });
                return "b";
            });

        var actual = await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("b");
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateClientStreaming2()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateClientStreaming2", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<int>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var stream = new[] { 1 }.AsAsyncEnumerable();

        _service
            .Setup(s => s.DuplicateClientStreaming(It.IsNotNull<IAsyncEnumerable<int>>()))
            .Returns<IAsyncEnumerable<int>>(async values =>
            {
                var items = await values.ToListAsync().ConfigureAwait(false);
                items.ShouldBe(new[] { 1 });
                return "b";
            });

        var actual = await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

        actual.Value1.ShouldBe("b");
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingConvert()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingConvert))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 2 }.AsAsyncEnumerable();

#pragma warning disable CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
        static async IAsyncEnumerable<string> Callback(IAsyncEnumerable<int> values, CancellationToken token)
#pragma warning restore CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 2 });

            yield return "2";
        }

        _service
            .Setup(s => s.DuplexStreamingConvert(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { "2" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingConvertAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingConvertAsync))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 2 }.AsAsyncEnumerable();

        static async Task<IAsyncEnumerable<string>> Callback(IAsyncEnumerable<int> values, CancellationToken token)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 2 });

            return new[] { "2" }.AsAsyncEnumerable();
        }

        _service
            .Setup(s => s.DuplexStreamingConvertAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { "2" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingConvertValueTaskAsync()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingConvertValueTaskAsync))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 2 }.AsAsyncEnumerable();

        static async ValueTask<IAsyncEnumerable<string>> Callback(IAsyncEnumerable<int> values, CancellationToken token)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 2 });

            return new[] { "2" }.AsAsyncEnumerable();
        }

        _service
            .Setup(s => s.DuplexStreamingConvertValueTaskAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { "2" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingHeaderParameters()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingHeaderParameters))
            .CreateDelegate<Func<IContract, Message<int, string>, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 2 }.AsAsyncEnumerable();

#pragma warning disable CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
        static async IAsyncEnumerable<string> Callback(IAsyncEnumerable<int> values, int m, string p)
#pragma warning restore CS8425 // Async-iterator member has one or more parameters of type 'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, so the cancellation token parameter from the generated 'IAsyncEnumerable<>.GetAsyncEnumerator' will be unconsumed
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 2 });

            yield return "2";
        }

        _service
            .Setup(s => s.DuplexStreamingHeaderParameters(It.IsNotNull<IAsyncEnumerable<int>>(), 1, "prefix"))
            .Returns<IAsyncEnumerable<int>, int, string>(Callback);

        var response = await call(_service.Object, new Message<int, string>(1, "prefix"), input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { "2" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateDuplexStreaming1()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateDuplexStreaming1", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<string>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<string>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<string>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { "a" }.AsAsyncEnumerable();

        static async IAsyncEnumerable<string> Callback(IAsyncEnumerable<string> values)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { "a" });

            yield return "b";
        }

        _service
            .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<string>>()))
            .Returns<IAsyncEnumerable<string>>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { "b" });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplicateDuplexStreaming2()
    {
        var call = ChannelType
            .InstanceMethod("DuplicateDuplexStreaming2", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<int>), typeof(ServerCallContext))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 1 }.AsAsyncEnumerable();

        static async IAsyncEnumerable<int> Callback(IAsyncEnumerable<int> values)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 1 });

            yield return 2;
        }

        _service
            .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<int>>()))
            .Returns<IAsyncEnumerable<int>>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldBeNull();
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { 2 });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingWithHeadersTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingWithHeadersTask))
            .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message<int, int>?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 1 }.AsAsyncEnumerable();

        static async Task<(int Value, IAsyncEnumerable<int> Stream, int Count)> Callback(IAsyncEnumerable<int> values, CancellationToken token)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 1 });

            return (1, new[] { 2 }.AsAsyncEnumerable(), 2);
        }

        _service
            .Setup(s => s.DuplexStreamingWithHeadersTask(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, CancellationToken>(Callback);

        var response = await call(_service.Object, null, input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldNotBeNull();
        response.Item1.Value1.ShouldBe(1);
        response.Item1.Value2.ShouldBe(2);
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { 2 });
        _service.VerifyAll();
    }

    [Test]
    public async Task DuplexStreamingWithHeadersValueTask()
    {
        var call = ChannelType
            .InstanceMethod(nameof(IContract.DuplexStreamingWithHeadersValueTask))
            .CreateDelegate<Func<IContract, Message<int, int>, IAsyncEnumerable<int>, ServerCallContext, ValueTask<(Message<int>?, IAsyncEnumerable<int>)>>>(Channel);
        TestOutput.WriteLine(call.Method.Disassemble());

        var input = new[] { 1 }.AsAsyncEnumerable();

        static async ValueTask<(IAsyncEnumerable<int> Stream, int Count)> Callback(IAsyncEnumerable<int> values, int value, int count, CancellationToken token)
        {
            var items = await values.ToListAsync().ConfigureAwait(false);
            items.ShouldBe(new[] { 1 });

            return (new[] { 2 }.AsAsyncEnumerable(), 1);
        }

        _service
            .Setup(s => s.DuplexStreamingWithHeadersValueTask(It.IsNotNull<IAsyncEnumerable<int>>(), 1, 2, _tokenSource.Token))
            .Returns<IAsyncEnumerable<int>, int, int, CancellationToken>(Callback);

        var response = await call(_service.Object, new Message<int, int>(1, 2), input, _serverCallContext.Object).ConfigureAwait(false);

        response.Item1.ShouldNotBeNull();
        response.Item1.Value1.ShouldBe(1);
        var outputValues = await response.Item2.ToListAsync().ConfigureAwait(false);
        outputValues.ShouldBe(new[] { 2 });
        _service.VerifyAll();
    }
}