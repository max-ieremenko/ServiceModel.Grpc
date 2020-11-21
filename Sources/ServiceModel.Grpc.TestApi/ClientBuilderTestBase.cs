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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.TestApi.Domain;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public abstract class ClientBuilderTestBase
    {
        protected Func<IContract> Factory { get; set; } = null!;

        protected Mock<CallInvoker> CallInvoker { get; private set; } = null!;

        protected CancellationTokenSource TokenSource { get; private set; } = null!;

        [SetUp]
        public void BeforeEachTest()
        {
            CallInvoker = new Mock<CallInvoker>(MockBehavior.Strict);
            TokenSource = new CancellationTokenSource();
        }

        [Test]
        public void Empty()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.Empty)).Disassemble());

            CallInvoker.SetupBlockingUnaryCall();

            Factory().Empty();

            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task EmptyAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.EmptyAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCall();

            await Factory().EmptyAsync();

            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task EmptyValueTaskAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.EmptyValueTaskAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCall();

            await Factory().EmptyValueTaskAsync();

            CallInvoker.VerifyAll();
        }

        [Test]
        public void EmptyContext()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.EmptyContext)).Disassemble());

            var options = new CallOptions(deadline: DateTime.Now.AddDays(1));

            CallInvoker.SetupBlockingUnaryCall(actual => actual.Deadline.ShouldBe(options.Deadline));

            Factory().EmptyContext(options);

            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task EmptyTokenAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.EmptyTokenAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCall(actual => actual.CancellationToken.ShouldBe(TokenSource.Token));

            await Factory().EmptyTokenAsync(TokenSource.Token);

            CallInvoker.VerifyAll();
        }

        [Test]
        public void ReturnStringContext()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ReturnString)).Disassemble());

            CallInvoker.SetupBlockingUnaryCallOut("a");

            var actual = Factory().ReturnString();

            actual.ShouldBe("a");
            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task ReturnStringAsyncNullServerContext()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ReturnStringAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCallOut("a");

            var actual = await Factory().ReturnStringAsync();

            actual.ShouldBe("a");
            CallInvoker.VerifyAll();
        }

        [Test]
        public void ReturnStringAsyncServerContextValueIsNotSupported()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ReturnStringAsync)).Disassemble());

            var context = new Mock<ServerCallContext>(MockBehavior.Strict);

            CallInvoker.SetupAsyncUnaryCall(actual => actual.CancellationToken.ShouldBe(TokenSource.Token));

            Assert.ThrowsAsync<NotSupportedException>(() => Factory().ReturnStringAsync(context.Object));
        }

        [Test]
        public async Task ReturnValueTaskBoolAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ReturnValueTaskBoolAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCallOut(true);

            var actual = await Factory().ReturnValueTaskBoolAsync();

            actual.ShouldBeTrue();
            CallInvoker.VerifyAll();
        }

        [Test]
        public void OneParameterContext()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.OneParameterContext)).Disassemble());

            var options = new CallOptions(deadline: DateTime.Now.AddDays(1));

            CallInvoker.SetupBlockingUnaryCallIn(3);

            Factory().OneParameterContext(options, 3);

            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task OneParameterAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.OneParameterAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCallIn(1.5);

            await Factory().OneParameterAsync(1.5);

            CallInvoker.VerifyAll();
        }

        [Test]
        public void AddTwoValues()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.AddTwoValues)).Disassemble());

            CallInvoker.SetupBlockingUnaryCallInOut(2, 3.5, 5.5);

            var actual = Factory().AddTwoValues(2, 3.5);

            actual.ShouldBe(5.5);
            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task ConcatThreeValueAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ConcatThreeValueAsync)).Disassemble());

            CallInvoker.SetupAsyncUnaryCallInOut(1, "a", 2L, "1a2", options => options.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().ConcatThreeValueAsync(1, "a", TokenSource.Token, 2);

            actual.ShouldBe("1a2");
            CallInvoker.VerifyAll();
        }

        [Test]
        public void DuplicateUnary1()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateUnary), Array.Empty<Type>()).Disassemble());

            CallInvoker.SetupBlockingUnaryCallOut("a");

            var actual = Factory().DuplicateUnary();

            actual.ShouldBe("a");
            CallInvoker.VerifyAll();
        }

        [Test]
        public void DuplicateUnary2()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateUnary), typeof(string)).Disassemble());

            CallInvoker.SetupBlockingUnaryCallInOut("a", "b");

            var actual = Factory().DuplicateUnary("a");

            actual.ShouldBe("b");
            CallInvoker.VerifyAll();
        }

        [Test]
        public async Task ServerStreamingRepeatValue()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ServerStreamingRepeatValue)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, 10);

            CallInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = Factory().ServerStreamingRepeatValue(1, 2, TokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(TokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task ServerStreamingRepeatValueAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ServerStreamingRepeatValueAsync)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, 10);

            CallInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().ServerStreamingRepeatValueAsync(1, 2, TokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(TokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task ServerStreamingRepeatValueValueTaskAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ServerStreamingRepeatValueValueTaskAsync)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, 10);

            CallInvoker.SetupAsyncServerStreamingCall(
                1,
                2,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().ServerStreamingRepeatValueValueTaskAsync(1, 2, TokenSource.Token);

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(TokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task DuplicateServerStreaming1()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateServerStreaming), Array.Empty<Type>()).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, "a");

            CallInvoker.SetupAsyncServerStreamingCall(responseStream.Object);

            var actual = await Factory().DuplicateServerStreaming().ToListAsync();

            actual.ShouldBe(new[] { "a" });
            responseStream.Verify();
        }

        [Test]
        public async Task DuplicateServerStreaming2()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateServerStreaming), typeof(string)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, "b");

            CallInvoker.SetupAsyncServerStreamingCall("a", responseStream.Object);

            var actual = await Factory().DuplicateServerStreaming("a").ToListAsync();

            actual.ShouldBe(new[] { "b" });
            responseStream.Verify();
        }

        [Test]
        public async Task EmptyServerStreaming()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.EmptyServerStreaming)).Disassemble());

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, 10);

            CallInvoker.SetupAsyncServerStreamingCall(responseStream.Object);

            var actual = Factory().EmptyServerStreaming();

            var content = new List<int>();
            await foreach (var i in actual.WithCancellation(TokenSource.Token))
            {
                content.Add(i);
            }

            content.ShouldBe(new[] { 10 });
            responseStream.Verify();
        }

        [Test]
        public async Task ClientStreamingSumValues()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ClientStreamingSumValues)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            CallInvoker.SetupAsyncClientStreamingCall(
                requestStream.Object,
                "3",
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().ClientStreamingSumValues(new[] { 1, 2 }.AsAsyncEnumerable(), TokenSource.Token);

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingEmpty()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ClientStreamingEmpty)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            CallInvoker.SetupAsyncClientStreamingCall(requestStream.Object);

            await Factory().ClientStreamingEmpty(new[] { 1, 2 }.AsAsyncEnumerable());

            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task ClientStreamingHeaderParameters()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.ClientStreamingHeaderParameters)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            CallInvoker.SetupAsyncClientStreamingCall(
                requestStream.Object,
                "sum-6",
                options =>
                {
                    var values = CompatibilityTools.GetMethodInputFromHeader<int, string>(DataContractMarshallerFactory.Default, options.Headers);
                    values.Value1.ShouldBe(2);
                    values.Value2.ShouldBe("sum-");
                });

            await Factory().ClientStreamingHeaderParameters(new[] { 1, 2 }.AsAsyncEnumerable(), 2, "sum-");

            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming1()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateClientStreaming), typeof(IAsyncEnumerable<string>)).Disassemble());

            var serverValues = new List<string>();

            var requestStream = new Mock<IClientStreamWriter<Message<string>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            CallInvoker.SetupAsyncClientStreamingCall(requestStream.Object, "3");

            var actual = await Factory().DuplicateClientStreaming(new[] { "a", "b" }.AsAsyncEnumerable());

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { "a", "b" });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateClientStreaming2()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateClientStreaming), typeof(IAsyncEnumerable<int>)).Disassemble());

            var serverValues = new List<int>();

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(serverValues);

            CallInvoker.SetupAsyncClientStreamingCall(requestStream.Object, "3");

            var actual = await Factory().DuplicateClientStreaming(new[] { 1, 2 }.AsAsyncEnumerable());

            actual.ShouldBe("3");
            serverValues.ShouldBe(new[] { 1, 2 });
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvert()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplexStreamingConvert)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = Factory().DuplexStreamingConvert(new[] { 1, 2 }.AsAsyncEnumerable(), TokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplexStreamingConvertAsync)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().DuplexStreamingConvertAsync(new[] { 1, 2 }.AsAsyncEnumerable(), TokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingConvertValueTaskAsync()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplexStreamingConvertValueTaskAsync)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(TokenSource.Token, requestValues, i => i.ToString());

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                o => o.CancellationToken.ShouldBe(TokenSource.Token));

            var actual = await Factory().DuplexStreamingConvertValueTaskAsync(new[] { 1, 2 }.AsAsyncEnumerable(), TokenSource.Token);

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "1", "2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplexStreamingHeaderParameters()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplexStreamingHeaderParameters)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => "prefix-" + i);

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                options =>
                {
                    var header = CompatibilityTools.GetMethodInputFromHeader<int, string>(DataContractMarshallerFactory.Default, options.Headers);
                    header.Value1.ShouldBe(1);
                    header.Value2.ShouldBe("prefix-");
                });

            var actual = Factory().DuplexStreamingHeaderParameters(new[] { 1, 2 }.AsAsyncEnumerable(), 1, "prefix-");

            var values = await actual.ToListAsync();
            values.ShouldBe(new[] { "prefix-1", "prefix-2" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming1()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IAsyncEnumerable<string>)).Disassemble());

            var requestValues = new List<string>();

            var responseStream = new Mock<IAsyncStreamReader<Message<string>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => i + "1");

            var requestStream = new Mock<IClientStreamWriter<Message<string>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object);

            var actual = await Factory().DuplicateDuplexStreaming(new[] { "a", "b" }.AsAsyncEnumerable()).ToListAsync();

            actual.ShouldBe(new[] { "a1", "b1" });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        [Test]
        public async Task DuplicateDuplexStreaming2()
        {
            Console.WriteLine(GetClientInstanceMethod(nameof(IContract.DuplicateDuplexStreaming), typeof(IAsyncEnumerable<int>)).Disassemble());

            var requestValues = new List<int>();

            var responseStream = new Mock<IAsyncStreamReader<Message<int>>>(MockBehavior.Strict);
            responseStream.Setup(default, requestValues, i => i + 1);

            var requestStream = new Mock<IClientStreamWriter<Message<int>>>(MockBehavior.Strict);
            requestStream.Setup(requestValues);

            CallInvoker.SetupAsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object);

            var actual = await Factory().DuplicateDuplexStreaming(new[] { 1, 2 }.AsAsyncEnumerable()).ToListAsync();

            actual.ShouldBe(new[] { 2, 3 });
            responseStream.Verify();
            requestStream.VerifyAll();
        }

        protected virtual MethodInfo GetClientInstanceMethod(string name)
        {
            return Factory().GetType().InstanceMethod(name);
        }

        protected virtual MethodInfo GetClientInstanceMethod(string name, params Type[] parameters)
        {
            return Factory().GetType().InstanceMethod(name, parameters);
        }
    }
}
