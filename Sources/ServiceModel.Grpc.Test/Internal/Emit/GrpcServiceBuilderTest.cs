using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class GrpcServiceBuilderTest
    {
        private GrpcServiceBuilder _sut;
        private Type _channelType;
        private Mock<IContract> _service;
        private CancellationTokenSource _tokenSource;

        [OneTimeSetUp]
        public void BeforeAllTest()
        {
            _sut = new GrpcServiceBuilder(typeof(IContract));

            foreach (var method in ReflectionTools.GetMethods(typeof(IContract)))
            {
                _sut.BuildCall(new MessageAssembler(method), method.Name);
            }

            _channelType = _sut.BuildType();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _service = new Mock<IContract>(MockBehavior.Strict);
            _tokenSource = new CancellationTokenSource();
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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);

            _service
                .Setup(s => s.EmptyContext(serverContext.Object));

            var actual = await call(_service.Object, new Message(), serverContext.Object);

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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            _service
                .Setup(s => s.EmptyTokenAsync(_tokenSource.Token))
                .Returns(Task.CompletedTask);

            var actual = await call(_service.Object, new Message(), serverContext.Object);

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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);

            _service
                .Setup(s => s.ReturnStringAsync(serverContext.Object))
                .Returns(Task.FromResult("a"));

            var actual = await call(_service.Object, new Message(), serverContext.Object);

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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);
            serverContext
                .Protected()
                .SetupGet<Metadata>("RequestHeadersCore")
                .Returns(new Metadata());
            serverContext
                .Protected()
                .SetupGet<DateTime>("DeadlineCore")
                .Returns(DateTime.Now);
            serverContext
                .Protected()
                .SetupGet<WriteOptions>("WriteOptionsCore")
                .Returns(WriteOptions.Default);

            _service
                .Setup(s => s.OneParameterContext(It.IsAny<CallOptions>(), 3))
                .Callback<CallOptions, int>((options, _) =>
                {
                    options.CancellationToken.ShouldBe(_tokenSource.Token);
                });

            var actual = await call(_service.Object, new Message<int>(3), serverContext.Object);

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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            _service
                .Setup(s => s.ConcatThreeValueAsync(1, "a", _tokenSource.Token, 3))
                .Returns(Task.FromResult("1a3"));

            var actual = await call(_service.Object, new Message<int, string, long>(1, "a", 3), serverContext.Object);

            actual.Value1.ShouldBe("1a3");
            _service.VerifyAll();
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.EmptyServerStreaming))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            var actual = new List<int>();

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            stream
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<int>>()))
                .Callback<Message<int>>(message =>
                {
                    actual.Add(message.Value1);
                })
                .Returns(Task.CompletedTask);

            _service
                .Setup(s => s.EmptyServerStreaming())
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message(), stream.Object, serverContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
            stream.VerifyAll();
            _service.VerifyAll();
            serverContext.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ServerStreamingRepeatValue))
                .CreateDelegate<ServerStreamingServerMethod<IContract, Message<int, int>, Message<int>>>();
            Console.WriteLine(call.Method.Disassemble());

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            var actual = new List<int>();

            var stream = new Mock<IServerStreamWriter<Message<int>>>(MockBehavior.Strict);
            stream
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<int>>()))
                .Callback<Message<int>>(message =>
                {
                    actual.Add(message.Value1);
                })
                .Returns(Task.CompletedTask);

            _service
                .Setup(s => s.ServerStreamingRepeatValue(1, 2, _tokenSource.Token))
                .Returns(new[] { 1, 2, 3 }.AsAsyncEnumerable());

            await call(_service.Object, new Message<int, int>(1, 2), stream.Object, serverContext.Object);

            actual.ShouldBe(new[] { 1, 2, 3 });
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

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream
                .Setup(s => s.MoveNext(_tokenSource.Token))
                .Callback(() =>
                {
                    stream.SetupGet(s => s.Current).Returns(new Message<int>(2));
                    stream.Setup(s => s.MoveNext(_tokenSource.Token)).Returns(Task.FromResult(false));
                })
                .Returns(Task.FromResult(true));

            _service
                .Setup(s => s.ClientStreamingEmpty(It.IsNotNull<IAsyncEnumerable<int>>()))
                .Callback<IAsyncEnumerable<int>>(async values =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(Task.CompletedTask);

            await call(_service.Object, stream.Object, serverContext.Object);

            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.ClientStreamingSumValues))
                .CreateDelegate<ClientStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            var stream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            stream
                .Setup(s => s.MoveNext(_tokenSource.Token))
                .Callback(() =>
                {
                    stream.SetupGet(s => s.Current).Returns(new Message<int>(2));
                    stream.Setup(s => s.MoveNext(_tokenSource.Token)).Returns(Task.FromResult(false));
                })
                .Returns(Task.FromResult(true));

            _service
                .Setup(s => s.ClientStreamingSumValues(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Returns<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                    return "2";
                });

            var actual = await call(_service.Object, stream.Object, serverContext.Object);

            actual.Value1.ShouldBe("2");
            stream.VerifyAll();
            _service.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvert()
        {
            var call = _channelType
                .StaticMethod(nameof(IContract.DuplexStreamingConvert))
                .CreateDelegate<DuplexStreamingServerMethod<IContract, Message<int>, Message<string>>>();
            Console.WriteLine(call.Method.Disassemble());

            var serverContext = new Mock<ServerCallContext>(MockBehavior.Strict);
            serverContext
                .Protected()
                .SetupGet<CancellationToken>("CancellationTokenCore")
                .Returns(_tokenSource.Token);

            var input = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            input
                .Setup(s => s.MoveNext(_tokenSource.Token))
                .Callback(() =>
                {
                    input.SetupGet(s => s.Current).Returns(new Message<int>(2));
                    input.Setup(s => s.MoveNext(_tokenSource.Token)).Returns(Task.FromResult(false));
                })
                .Returns(Task.FromResult(true));

            var outputValues = new List<string>();

            var output = new Mock<IServerStreamWriter<Message<string>>>(MockBehavior.Strict);
            output
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<string>>()))
                .Callback<Message<string>>(message =>
                {
                    outputValues.Add(message.Value1);
                })
                .Returns(Task.CompletedTask);

            _service
                .Setup(s => s.DuplexStreamingConvert(It.IsNotNull<IAsyncEnumerable<int>>(), _tokenSource.Token))
                .Callback<IAsyncEnumerable<int>, CancellationToken>(async (values, _) =>
                {
                    var items = await values.ToListAsync();
                    items.ShouldBe(new[] { 2 });
                })
                .Returns(new[] { "2" }.AsAsyncEnumerable());

            await call(_service.Object, input.Object, output.Object, serverContext.Object);

            outputValues.ShouldBe(new[] { "2" });
            input.VerifyAll();
            output.VerifyAll();
            _service.VerifyAll();
        }
    }
}
