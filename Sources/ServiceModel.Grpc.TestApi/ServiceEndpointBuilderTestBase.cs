// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
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
        }

        [Test]
        public async Task Empty()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.Empty))
                .CreateDelegate<Func<IContract, Message, ServerCallContext, Task<Message>>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.DuplicateUnary("a"))
                .Returns("b");

            var actual = await call(_service.Object, new Message<string>("a"), null!).ConfigureAwait(false);

            actual.Value1.ShouldBe("b");
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.EmptyServerStreaming))
                .CreateDelegate<Func<IContract, Message, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.EmptyServerStreaming())
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message(), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ServerStreamingRepeatValue))
                .CreateDelegate<Func<IContract, Message<int, int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValue(1, 2, _tokenSource.Token))
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValueAsync()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ServerStreamingRepeatValueAsync))
                .CreateDelegate<Func<IContract, Message<int, int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValueAsync(1, 2, _tokenSource.Token))
                .Returns(Task.FromResult(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValueValueTaskAsync()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ServerStreamingRepeatValueValueTaskAsync))
                .CreateDelegate<Func<IContract, Message<int, int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValueValueTaskAsync(1, 2, _tokenSource.Token))
                .Returns(new ValueTask<IAsyncEnumerable<int>>(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingWithHeadersTask()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ServerStreamingWithHeadersTask))
                .CreateDelegate<Func<IContract, Message<int, int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingWithHeadersTask(1, 2, _tokenSource.Token))
                .Returns(Task.FromResult((1, new[] { 1, 2, 3 }.AsAsyncEnumerable(), 2)));

            _serverCallContext
                .Protected()
                .Setup<Task>("WriteResponseHeadersAsyncCore", ItExpr.IsAny<Metadata>())
                .Callback<Metadata>(responseHeaders =>
                {
                    responseHeaders.ShouldNotBeNull();
                    var headers = CompatibilityToolsTestExtensions.DeserializeMethodOutput<int, int>(DataContractMarshallerFactory.Default, responseHeaders);
                    headers.Value1.ShouldBe(1);
                    headers.Value2.ShouldBe(2);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.Verify();
        }

        [Test]
        public async Task ServerStreamingWithHeadersValueTask()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ServerStreamingWithHeadersValueTask))
                .CreateDelegate<Func<IContract, Message<int, int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingWithHeadersValueTask(1, 2, _tokenSource.Token))
                .Returns(new ValueTask<(IAsyncEnumerable<int> Stream, int Count)>((new[] { 1, 2, 3 }.AsAsyncEnumerable(), 2)));

            _serverCallContext
                .Protected()
                .Setup<Task>("WriteResponseHeadersAsyncCore", ItExpr.IsAny<Metadata>())
                .Callback<Metadata>(responseHeaders =>
                {
                    responseHeaders.ShouldNotBeNull();
                    var header = CompatibilityToolsTestExtensions.DeserializeMethodOutput<int>(DataContractMarshallerFactory.Default, responseHeaders);
                    header.ShouldBe(2);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.Verify();
        }

        [Test]
        public async Task DuplicateServerStreaming1()
        {
            var call = ChannelType
                .InstanceMethod("DuplicateServerStreaming1", typeof(IContract), typeof(Message), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<Func<IContract, Message, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.DuplicateServerStreaming())
                .Returns(new[] { "a" }.AsAsyncEnumerable());

            await call(_service.Object, new Message(), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { "a" });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateServerStreaming2()
        {
            var call = ChannelType
                .InstanceMethod("DuplicateServerStreaming2", typeof(IContract), typeof(Message<string>), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<Func<IContract, Message<string>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.DuplicateServerStreaming("b"))
                .Returns(new[] { "a" }.AsAsyncEnumerable());

            await call(_service.Object, new Message<string>("b"), stream.Object, _serverCallContext.Object).ConfigureAwait(false);

            actual.ShouldBe(new[] { "a" });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingEmpty()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ClientStreamingEmpty))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message>>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var stream = new[] { 2 }.AsAsyncEnumerable();

            _service
                .Setup(s => s.ClientStreamingEmpty(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Callback<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(Task.CompletedTask);

            await call(_service.Object, null, stream, _serverCallContext.Object).ConfigureAwait(false);

            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ClientStreamingSumValues))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

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
        public async Task ClientStreamingHeaderParameters()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.ClientStreamingHeaderParameters))
                .CreateDelegate<Func<IContract, Message<int, string>, IAsyncEnumerable<int>, ServerCallContext, Task<Message<string>>>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
            Console.WriteLine(call.Method.Disassemble());

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
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 2 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvert(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new[] { "2" }.AsAsyncEnumerable());

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { "2" });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertAsync()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.DuplexStreamingConvertAsync))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 2 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvertAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(Task.FromResult(new[] { "2" }.AsAsyncEnumerable()));

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { "2" });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertValueTaskAsync()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.DuplexStreamingConvertValueTaskAsync))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 2 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvertValueTaskAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new ValueTask<IAsyncEnumerable<string>>(new[] { "2" }.AsAsyncEnumerable()));

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { "2" });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingHeaderParameters()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.DuplexStreamingHeaderParameters))
                .CreateDelegate<Func<IContract, Message<int, string>, IAsyncEnumerable<int>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 2 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingHeaderParameters(It.IsNotNull<IAsyncEnumerable<int>>(), 1, "prefix"))
                .Callback<IAsyncEnumerable<int>, int, string>(async (values, m, p) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new[] { "2" }.AsAsyncEnumerable());

            await call(_service.Object, new Message<int, string>(1, "prefix"), input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { "2" });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming1()
        {
            var call = ChannelType
                .InstanceMethod("DuplicateDuplexStreaming1", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<string>), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<string>, IServerStreamWriter<Message<string>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { "a" }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<string>>()))
                .Callback<IAsyncEnumerable<string>>(async values =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { "a" });
                })
                .Returns(new[] { "b" }.AsAsyncEnumerable());

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { "b" });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming2()
        {
            var call = ChannelType
                .InstanceMethod("DuplicateDuplexStreaming2", typeof(IContract), typeof(Message), typeof(IAsyncEnumerable<int>), typeof(IServerStreamWriter<Message<int>>), typeof(ServerCallContext))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 1 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Callback<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 1 });
                })
                .Returns(new[] { 2 }.AsAsyncEnumerable());

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { 2 });
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingWithHeadersTask()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.DuplexStreamingWithHeadersTask))
                .CreateDelegate<Func<IContract, Message?, IAsyncEnumerable<int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 1 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingWithHeadersTask(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 1 });
                })
                .ReturnsAsync((1, new[] { 2 }.AsAsyncEnumerable(), 2));

            _serverCallContext
                .Protected()
                .Setup<Task>("WriteResponseHeadersAsyncCore", ItExpr.IsAny<Metadata>())
                .Callback<Metadata>(responseHeaders =>
                {
                    responseHeaders.ShouldNotBeNull();
                    var headers = CompatibilityToolsTestExtensions.DeserializeMethodOutput<int, int>(DataContractMarshallerFactory.Default, responseHeaders);
                    headers.Value1.ShouldBe(1);
                    headers.Value2.ShouldBe(2);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await call(_service.Object, null, input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { 2 });
            output.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.Verify();
        }

        [Test]
        public async Task DuplexStreamingWithHeadersValueTask()
        {
            var call = ChannelType
                .InstanceMethod(nameof(IContract.DuplexStreamingWithHeadersValueTask))
                .CreateDelegate<Func<IContract, Message<int, int>, IAsyncEnumerable<int>, IServerStreamWriter<Message<int>>, ServerCallContext, Task>>(Channel);
            Console.WriteLine(call.Method.Disassemble());

            var input = new[] { 1 }.AsAsyncEnumerable();

            var output = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingWithHeadersValueTask(It.IsNotNull<IAsyncEnumerable<int>>(), 1, 2, _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, int, int, CancellationToken>(async (values, value, count, _) =>
                {
                    var items = await values.ToListAsync().ConfigureAwait(false);
                    items.ShouldBe(new[] { 1 });
                })
                .Returns(new ValueTask<(IAsyncEnumerable<int> Stream, int Count)>((new[] { 2 }.AsAsyncEnumerable(), 1)));

            _serverCallContext
                .Protected()
                .Setup<Task>("WriteResponseHeadersAsyncCore", ItExpr.IsAny<Metadata>())
                .Callback<Metadata>(responseHeaders =>
                {
                    responseHeaders.ShouldNotBeNull();
                    var header = CompatibilityToolsTestExtensions.DeserializeMethodOutput<int>(DataContractMarshallerFactory.Default, responseHeaders);
                    header.ShouldBe(1);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await call(_service.Object, new Message<int, int>(1, 2), input, output.Object, _serverCallContext.Object).ConfigureAwait(false);

            outputValues.ShouldBe(new[] { 2 });
            output.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.Verify();
        }
    }
}
