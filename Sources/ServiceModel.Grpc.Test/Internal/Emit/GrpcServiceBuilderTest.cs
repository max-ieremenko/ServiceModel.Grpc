// <copyright>
// Copyright 2020 Max Ieremenko
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
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class GrpcServiceBuilderTest
    {
        private Type _channelType;
        private Mock<IContract> _service;
        private CancellationTokenSource _tokenSource;
        private Mock<ServerCallContext> _serverCallContext;

        [OneTimeSetUp]
        public void BeforeAllTest()
        {
            var sut = new GrpcServiceBuilder(typeof(IContract), DataContractMarshallerFactory.Default, nameof(GrpcServiceBuilderTest));

            foreach (var method in ReflectionTools.GetMethods(typeof(IContract)))
            {
                sut.BuildCall(new MessageAssembler(method), method.Name);
            }

            _channelType = sut.BuildType();
        }

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
            var call = _channelType
                .StaticMethod(nameof(IContract.Empty))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.Empty());

            var actual = await call(_service.Object, new Message(), null);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.EmptyAsync())
                .Returns(Task.CompletedTask);

            var actual = await call(_service.Object, new Message(), null);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyValueTaskAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyValueTaskAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.EmptyValueTaskAsync())
                .Returns(new ValueTask(Task.CompletedTask));

            var actual = await call(_service.Object, new Message(), null);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyContext()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyContext))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.EmptyContext(It.IsNotNull<CallContext>()))
                .Callback<CallContext>(c =>
                {
                    c.ServerCallContext.ShouldBe(_serverCallContext.Object);
                });

            var actual = await call(_service.Object, new Message(), _serverCallContext.Object);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyTokenAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyTokenAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.EmptyTokenAsync(_tokenSource.Token))
                .Returns(Task.CompletedTask);

            var actual = await call(_service.Object, new Message(), _serverCallContext.Object);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task ReturnString()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ReturnString))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.ReturnString())
                .Returns("a");

            var actual = await call(_service.Object, new Message(), null);

            actual.Value1.ShouldBe("a");
            _service.VerifyAll();
        }

        [Test]
        public async Task ReturnStringAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ReturnStringAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.ReturnStringAsync(_serverCallContext.Object))
                .Returns(Task.FromResult("a"));

            var actual = await call(_service.Object, new Message(), _serverCallContext.Object);

            actual.Value1.ShouldBe("a");
            _service.VerifyAll();
        }

        [Test]
        public async Task ReturnValueTaskBoolAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ReturnValueTaskBoolAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message<bool>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.ReturnValueTaskBoolAsync())
                .Returns(new ValueTask<bool>(Task.FromResult(true)));

            var actual = await call(_service.Object, new Message(), null);

            actual.Value1.ShouldBeTrue();
            _service.VerifyAll();
        }

        [Test]
        public async Task OneParameterContext()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.OneParameterContext))
                .CreateDelegate<UnaryServerMethod<IContract, Message<int>, Message>>();
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

            var actual = await call(_service.Object, new Message<int>(3), _serverCallContext.Object);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task OneParameterAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.OneParameterAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message<double>, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.OneParameterAsync(3.5))
                .Returns(Task.CompletedTask);

            var actual = await call(_service.Object, new Message<double>(3.5), null);

            actual.ShouldNotBeNull();
            _service.VerifyAll();
        }

        [Test]
        public async Task AddTwoValues()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.AddTwoValues))
                .CreateDelegate<UnaryServerMethod<IContract, Message<int, double>, Message<double>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.AddTwoValues(1, 3.5))
                .Returns(4.5);

            var actual = await call(_service.Object, new Message<int, double>(1, 3.5), null);

            actual.Value1.ShouldBe(4.5);
            _service.VerifyAll();
        }

        [Test]
        public async Task ConcatThreeValueAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ConcatThreeValueAsync))
                .CreateDelegate<UnaryServerMethod<IContract, Message<int, string, long>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.ConcatThreeValueAsync(1, "a", _tokenSource.Token, 3))
                .Returns(Task.FromResult("1a3"));

            var actual = await call(_service.Object, new Message<int, string, long>(1, "a", 3), _serverCallContext.Object);

            actual.Value1.ShouldBe("1a3");
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateUnary1()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateUnary), typeof(IContract), typeof(Message), typeof(ServerCallContext))
                .CreateDelegate<UnaryServerMethod<IContract, Message, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.DuplicateUnary())
                .Returns("a");

            var actual = await call(_service.Object, new Message(), null);

            actual.Value1.ShouldBe("a");
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateUnary2()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateUnary), typeof(IContract), typeof(Message<string>), typeof(ServerCallContext))
                .CreateDelegate<UnaryServerMethod<IContract, Message<string>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _service
                .Setup(s => s.DuplicateUnary("a"))
                .Returns("b");

            var actual = await call(_service.Object, new Message<string>("a"), null);

            actual.Value1.ShouldBe("b");
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyServerStreaming))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.EmptyServerStreaming())
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message(), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
            _serverCallContext.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ServerStreamingRepeatValue))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message<int, int>, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValue(1, 2, _tokenSource.Token))
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValueAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ServerStreamingRepeatValueAsync))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message<int, int>, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValueAsync(1, 2, _tokenSource.Token))
                .Returns(Task.FromResult(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValueValueTaskAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ServerStreamingRepeatValueValueTaskAsync))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message<int, int>, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.ServerStreamingRepeatValueValueTaskAsync(1, 2, _tokenSource.Token))
                .Returns(new ValueTask<IAsyncEnumerable<int>>(new[] { 1, 2, 3 }.AsAsyncEnumerable()));

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateServerStreaming1()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateServerStreaming), typeof(IContract), typeof(Message), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.DuplicateServerStreaming())
                .Returns(new[] { "a" }.AsAsyncEnumerable());

            await call(_service.Object, new Message(), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { "a" });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateServerStreaming2()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateServerStreaming), typeof(IContract), typeof(Message<string>), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message<string>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var actual = stream.Setup();

            _service
                .Setup(s => s.DuplicateServerStreaming("b"))
                .Returns(new[] { "a" }.AsAsyncEnumerable());

            await call(_service.Object, new Message<string>("b"), stream.Object, _serverCallContext.Object);

            actual.ShouldBe(new[] { "a" });
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingEmpty()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ClientStreamingEmpty))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<int>, Message>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream.Setup(_tokenSource.Token, 2);

            _service
                .Setup(s => s.ClientStreamingEmpty(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Callback<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(Task.CompletedTask);

            await call(_service.Object, stream.Object, _serverCallContext.Object);

            stream.Verify();
            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ClientStreamingSumValues))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream.Setup(_tokenSource.Token, 2);

            _service
                .Setup(s => s.ClientStreamingSumValues(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Returns<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                    return "2";
                });

            var actual = await call(_service.Object, stream.Object, _serverCallContext.Object);

            actual.Value1.ShouldBe("2");
            stream.Verify();
            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingHeaderParameters()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ClientStreamingHeaderParameters))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _serverCallContext
                .Protected()
                .SetupGet<Metadata>("RequestHeadersCore")
                .Returns(CompatibilityTools.MethodInputAsHeader(DataContractMarshallerFactory.Default, 1, "prefix"));

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream.Setup(_tokenSource.Token, 2);

            _service
                .Setup(s => s.ClientStreamingHeaderParameters(It.IsNotNull<IAsyncEnumerable<int>>(), 1, "prefix"))
                .Returns<IAsyncEnumerable<int>, int, string>(async (values, m, p) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                    return "2";
                });

            var actual = await call(_service.Object, stream.Object, _serverCallContext.Object);

            actual.Value1.ShouldBe("2");
            stream.Verify();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming1()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateClientStreaming), typeof(IContract), typeof(IAsyncStreamReader<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<string>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            stream.Setup(_tokenSource.Token, "a");

            _service
                .Setup(s => s.DuplicateClientStreaming(It.IsNotNull<IAsyncEnumerable<string>>()))
                .Returns<IAsyncEnumerable<string>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { "a" });
                    return "b";
                });

            var actual = await call(_service.Object, stream.Object, _serverCallContext.Object);

            actual.Value1.ShouldBe("b");
            stream.Verify();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming2()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateClientStreaming), typeof(IContract), typeof(IAsyncStreamReader<Message<int>>), typeof(ServerCallContext))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream.Setup(_tokenSource.Token, 1);

            _service
                .Setup(s => s.DuplicateClientStreaming(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Returns<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 1 });
                    return "b";
                });

            var actual = await call(_service.Object, stream.Object, _serverCallContext.Object);

            actual.Value1.ShouldBe("b");
            stream.Verify();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvert()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplexStreamingConvert))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, 2);

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvert(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new[] { "2" }.AsAsyncEnumerable());

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { "2" });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplexStreamingConvertAsync))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, 2);

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvertAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(Task.FromResult(new[] { "2" }.AsAsyncEnumerable()));

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { "2" });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertValueTaskAsync()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplexStreamingConvertValueTaskAsync))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, 2);

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingConvertValueTaskAsync(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new ValueTask<IAsyncEnumerable<string>>(new[] { "2" }.AsAsyncEnumerable()));

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { "2" });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingHeaderParameters()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplexStreamingHeaderParameters))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            _serverCallContext
                .Protected()
                .SetupGet<Metadata>("RequestHeadersCore")
                .Returns(CompatibilityTools.MethodInputAsHeader(DataContractMarshallerFactory.Default, 1, "prefix"));

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, 2);

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplexStreamingHeaderParameters(It.IsNotNull<IAsyncEnumerable<int>>(), 1, "prefix"))
                .Callback<IAsyncEnumerable<int>, int, string>(async (values, m, p) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new[] { "2" }.AsAsyncEnumerable());

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { "2" });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming1()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IContract), typeof(IAsyncStreamReader<Message<string>>), typeof(IServerStreamWriter<Message<string>>), typeof(ServerCallContext))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<string>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var input = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, "a");

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<string>>()))
                .Callback<IAsyncEnumerable<string>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { "a" });
                })
                .Returns(new[] { "b" }.AsAsyncEnumerable());

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { "b" });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming2()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IContract), typeof(IAsyncStreamReader<Message<int>>), typeof(IServerStreamWriter<Message<int>>), typeof(ServerCallContext))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input.Setup(_tokenSource.Token, 1);

            var output = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            var outputValues = output.Setup();

            _service
                .Setup(s => s.DuplicateDuplexStreaming(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Callback<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 1 });
                })
                .Returns(new[] { 2 }.AsAsyncEnumerable());

            await call(_service.Object, input.Object, output.Object, _serverCallContext.Object);

            outputValues.ShouldBe(new[] { 2 });
            input.Verify();
            output.VerifyAll();
            _service.VerifyAll();
        }
    }
}
