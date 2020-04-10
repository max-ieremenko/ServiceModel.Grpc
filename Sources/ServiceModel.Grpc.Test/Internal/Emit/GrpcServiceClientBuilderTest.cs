using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.TestApi;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    [TestFixture]
    public class GrpcServiceClientBuilderTest
    {
        private Func<IContract> _factory;
        private Mock<CallInvoker> _callInvoker;
        private CancellationTokenSource _tokenSource;

        [OneTimeSetUp]
        public void BeforeAllTests()
        {
            var factory = new GrpcServiceClientBuilder<IContract>()
                .Build(DataContractMarshallerFactory.Default);

            _factory = () => factory(_callInvoker.Object);
        }

        [SetUp]
        public void BeforeEachTest()
        {
            _callInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            _tokenSource = new CancellationTokenSource();
        }

        [Test]
        public void Empty()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.Empty)).Disassemble());

            _callInvoker.SetupBlockingUnaryCall();

            _factory().Empty();

            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task EmptyAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCall();

            await _factory().EmptyAsync();

            _callInvoker.VerifyAll();
        }

        [Test]
        public void EmptyContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyContext)).Disassemble());

            var options = new CallOptions().WithDeadline(DateTime.Now.AddDays(1));

            _callInvoker.SetupBlockingUnaryCall(actual => actual.Deadline.ShouldBe(options.Deadline));

            _factory().EmptyContext(options);

            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task EmptyTokenAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyTokenAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCall(actual => actual.CancellationToken.ShouldBe(_tokenSource.Token));

            await _factory().EmptyTokenAsync(_tokenSource.Token);

            _callInvoker.VerifyAll();
        }

        [Test]
        public void ReturnStringContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ReturnString)).Disassemble());

            _callInvoker.SetupBlockingUnaryCallOut("a");

            var actual = _factory().ReturnString();

            actual.ShouldBe("a");
            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task ReturnStringAsyncNullServerContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ReturnStringAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCallOut("a");

            var actual = await _factory().ReturnStringAsync();

            actual.ShouldBe("a");
            _callInvoker.VerifyAll();
        }

        [Test]
        public void ReturnStringAsyncServerContextValueIsNotSupported()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ReturnStringAsync)).Disassemble());

            var context = new Mock<ServerCallContext>(MockBehavior.Strict);

            _callInvoker.SetupAsyncUnaryCall(actual => actual.CancellationToken.ShouldBe(_tokenSource.Token));

            Assert.ThrowsAsync<NotSupportedException>(() => _factory().ReturnStringAsync(context.Object));
        }

        [Test]
        public void OneParameterContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.OneParameterContext)).Disassemble());

            var options = new CallOptions().WithDeadline(DateTime.Now.AddDays(1));

            _callInvoker.SetupBlockingUnaryCallIn(3);

            _factory().OneParameterContext(options, 3);

            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task OneParameterAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.OneParameterAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCallIn(1.5);

            await _factory().OneParameterAsync(1.5);

            _callInvoker.VerifyAll();
        }

        [Test]
        public void AddTwoValues()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.AddTwoValues)).Disassemble());

            _callInvoker.SetupBlockingUnaryCallInOut(2, 3.5, 5.5);

            var actual = _factory().AddTwoValues(2, 3.5);

            actual.ShouldBe(5.5);
            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task ConcatThreeValueAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ConcatThreeValueAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCallInOut(1, "a", 2L, "1a2", options => options.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().ConcatThreeValueAsync(1, "a", _tokenSource.Token, 2);

            actual.ShouldBe("1a2");
            _callInvoker.VerifyAll();
        }

        [Test]
        public void DisposableNotSupported()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.Dispose)).Disassemble());

            var ex = Assert.Throws<NotSupportedException>(_factory().Dispose);

            ex.Message.ShouldContain("System.IDisposable.Dispose");
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ServerStreamingRepeatValue)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream
                .Setup(r => r.MoveNext(_tokenSource.Token))
                .Callback(() =>
                {
                    responseStream
                        .SetupGet(r => r.Current)
                        .Returns(new Message<int>(10));

                    responseStream
                        .Setup(r => r.MoveNext(_tokenSource.Token))
                        .Returns(Task.FromResult(false));
                })
                .Returns(Task.FromResult(true));

            _callInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = _factory().ServerStreamingRepeatValue(1, 2, _tokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(_tokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyServerStreaming)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream
                .Setup(r => r.MoveNext(default))
                .Callback(() =>
                {
                    responseStream
                        .SetupGet(r => r.Current)
                        .Returns(new Message<int>(10));

                    responseStream
                        .Setup(r => r.MoveNext(default))
                        .Returns(Task.FromResult(false));
                })
                .Returns(Task.FromResult(true));

            _callInvoker.SetupAsyncServerStreamingCall(responseStream.Object);

            var actual = _factory().EmptyServerStreaming();

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(_tokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ClientStreamingSumValues)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<int>>()))
                .Callback<Message<int>>(message => serverValues.Add(message.Value1))
                .Returns(Task.CompletedTask);
            requestStream
                .Setup(s => s.CompleteAsync())
                .Returns(Task.CompletedTask);

            _callInvoker.SetupAsyncClientStreamingCall(
                requestStream.Object,
                "3",
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().ClientStreamingSumValues(new[] { 1, 2 }.AsAsyncEnumerable(), _tokenSource.Token);

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingEmpty()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ClientStreamingEmpty)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<int>>()))
                .Callback<Message<int>>(message => serverValues.Add(message.Value1))
                .Returns(Task.CompletedTask);
            requestStream
                .Setup(s => s.CompleteAsync())
                .Returns(Task.CompletedTask);

            _callInvoker.SetupAsyncClientStreamingCall(requestStream.Object);

            await _factory().ClientStreamingEmpty(new[] { 1, 2 }.AsAsyncEnumerable());

            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvert()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplexStreamingConvert)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream
                .Setup(r => r.MoveNext(_tokenSource.Token))
                .Returns(() =>
                {
                    if (requestValues.Count == 0)
                    {
                        responseStream.SetupGet(s => s.Current).Throws<NotSupportedException>();
                        return Task.FromResult(false);
                    }

                    responseStream.SetupGet(s => s.Current).Returns(new Message<string>(requestValues[0].ToString()));
                    requestValues.RemoveAt(0);
                    return Task.FromResult(true);
                });

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream
                .Setup(s => s.WriteAsync(It.IsNotNull<Message<int>>()))
                .Callback<Message<int>>(message => requestValues.Add(message.Value1))
                .Returns(Task.CompletedTask);
            requestStream
                .Setup(s => s.CompleteAsync())
                .Returns(Task.CompletedTask);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = _factory().DuplexStreamingConvert(new[] { 1, 2 }.AsAsyncEnumerable(), _tokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            requestStream.VerifyAll();
        }
    }
}
