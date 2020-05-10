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
            var builder = new GrpcServiceClientBuilder
            {
                MarshallerFactory = DataContractMarshallerFactory.Default
            };

            var factory = builder.Build<IContract>(nameof(GrpcServiceClientBuilderTest));

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
        public async Task EmptyValueTaskAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyValueTaskAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCall();

            await _factory().EmptyValueTaskAsync();

            _callInvoker.VerifyAll();
        }

        [Test]
        public void EmptyContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyContext)).Disassemble());

            var options = new CallOptions(deadline: DateTime.Now.AddDays(1));

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
        public async Task ReturnValueTaskBoolAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ReturnValueTaskBoolAsync)).Disassemble());

            _callInvoker.SetupAsyncUnaryCallOut(true);

            var actual = await _factory().ReturnValueTaskBoolAsync();

            actual.ShouldBeTrue();
            _callInvoker.VerifyAll();
        }

        [Test]
        public void OneParameterContext()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.OneParameterContext)).Disassemble());

            var options = new CallOptions(deadline: DateTime.Now.AddDays(1));

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
        public void DuplicateUnary1()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateUnary), Array.Empty<Type>()).Disassemble());

            _callInvoker.SetupBlockingUnaryCallOut("a");

            var actual = _factory().DuplicateUnary();

            actual.ShouldBe("a");
            _callInvoker.VerifyAll();
        }

        [Test]
        public void DuplicateUnary2()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateUnary), typeof(string)).Disassemble());

            _callInvoker.SetupBlockingUnaryCallInOut("a", "b");

            var actual = _factory().DuplicateUnary("a");

            actual.ShouldBe("b");
            _callInvoker.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ServerStreamingRepeatValue)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, 10);

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
            responseStream.Verify();
        }

        [Test]
        public async Task ServerStreamingRepeatValueAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ServerStreamingRepeatValueAsync)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, 10);

            _callInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().ServerStreamingRepeatValueAsync(1, 2, _tokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(_tokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task ServerStreamingRepeatValueValueTaskAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ServerStreamingRepeatValueValueTaskAsync)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, 10);

            _callInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().ServerStreamingRepeatValueValueTaskAsync(1, 2, _tokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(_tokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task DuplicateServerStreaming1()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateServerStreaming), Array.Empty<Type>()).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, "a");

            _callInvoker.SetupAsyncServerStreamingCall(responseStream.Object);

            var actual = await _factory().DuplicateServerStreaming().ToListAsync();

            actual.ShouldBe(new[] { "a" });
            responseStream.Verify();
        }

        [Test]
        public async Task DuplicateServerStreaming2()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateServerStreaming), typeof(string)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, "b");

            _callInvoker.SetupAsyncServerStreamingCall("a", responseStream.Object);

            var actual = await _factory().DuplicateServerStreaming("a").ToListAsync();

            actual.ShouldBe(new[] { "b" });
            responseStream.Verify();
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.EmptyServerStreaming)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, 10);

            _callInvoker.SetupAsyncServerStreamingCall(responseStream.Object);

            var actual = _factory().EmptyServerStreaming();

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(_tokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ClientStreamingSumValues)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

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
            requestStream.Setup(serverValues);

            _callInvoker.SetupAsyncClientStreamingCall(requestStream.Object);

            await _factory().ClientStreamingEmpty(new[] { 1, 2 }.AsAsyncEnumerable());

            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingHeaderParameters()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.ClientStreamingHeaderParameters)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            _callInvoker.SetupAsyncClientStreamingCall(
                requestStream.Object,
                "sum-6",
                options =>
                {
                    var values = CompatibilityTools.GetMethodInputFromHeader<int, string>(DataContractMarshallerFactory.Default, options.Headers);
                    values.Item1.ShouldBe(2);
                    values.Item2.ShouldBe("sum-");
                });

            await _factory().ClientStreamingHeaderParameters(new[] { 1, 2 }.AsAsyncEnumerable(), 2, "sum-");

            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming1()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateClientStreaming), typeof(IAsyncEnumerable<string>)).Disassemble());

            var serverValues = new List<string>();

            var requestStream = new Mock<IClientStreamWriter<Message<string>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            _callInvoker.SetupAsyncClientStreamingCall(requestStream.Object, "3");

            var actual = await _factory().DuplicateClientStreaming(new[] { "a", "b" }.AsAsyncEnumerable());

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { "a", "b" });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming2()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateClientStreaming), typeof(IAsyncEnumerable<int>)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            _callInvoker.SetupAsyncClientStreamingCall(requestStream.Object, "3");

            var actual = await _factory().DuplicateClientStreaming(new[] { 1, 2 }.AsAsyncEnumerable());

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvert()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplexStreamingConvert)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = _factory().DuplexStreamingConvert(new[] { 1, 2 }.AsAsyncEnumerable(), _tokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplexStreamingConvertAsync)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().DuplexStreamingConvertAsync(new[] { 1, 2 }.AsAsyncEnumerable(), _tokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertValueTaskAsync()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplexStreamingConvertValueTaskAsync)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(_tokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(_tokenSource.Token));

            var actual = await _factory().DuplexStreamingConvertValueTaskAsync(new[] { 1, 2 }.AsAsyncEnumerable(), _tokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingHeaderParameters()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplexStreamingHeaderParameters)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => "prefix-" + i);

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                options =>
                {
                    var header = CompatibilityTools.GetMethodInputFromHeader<int, string>(DataContractMarshallerFactory.Default, options.Headers);
                    header.Item1.ShouldBe(1);
                    header.Item2.ShouldBe("prefix-");
                });

            var actual = _factory().DuplexStreamingHeaderParameters(new[] { 1, 2 }.AsAsyncEnumerable(), 1, "prefix-");

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "prefix-1", "prefix-2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming1()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IAsyncEnumerable<string>)).Disassemble());

            var requestValues = new List<string>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => i + "1");

            var requestStream = new Mock<IClientStreamWriter<Message<string>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object);

            var actual = await _factory().DuplicateDuplexStreaming(new[] { "a", "b" }.AsAsyncEnumerable()).ToListAsync();

            actual.ShouldBe(new[] { "a1", "b1" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming2()
        {
            Console.WriteLine(_factory().GetType().InstanceMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IAsyncEnumerable<int>)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => i + 1);

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            _callInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object);

            var actual = await _factory().DuplicateDuplexStreaming(new[] { 1, 2 }.AsAsyncEnumerable()).ToListAsync();

            actual.ShouldBe(new[] { 2, 3 });
            responseStream.Verify();
            requestStream.VerifyAll();
        }
    }
}
