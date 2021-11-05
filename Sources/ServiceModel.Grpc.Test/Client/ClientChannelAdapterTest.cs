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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.Client
{
    [TestFixture]
    public class ClientChannelAdapterTest
    {
        [Test]
        public void AsyncUnaryCallWaitResponseHeaders()
        {
            var expectedHeaders = new Metadata();
            var expectedStatus = new Status(StatusCode.Internal, "some details");
            var expectedTrailers = new Metadata();
            var disposeCounter = 0;

            var context = new CallContext(headers: null);

            var call = new AsyncUnaryCall<Message>(
                GetAsync(new Message()),
                GetAsync(expectedHeaders),
                () => expectedStatus,
                () => expectedTrailers,
                () => disposeCounter++);

            using (new SynchronizationContextMock())
            {
                ClientChannelAdapter.AsyncUnaryCallWait(call, context, CancellationToken.None).Wait();
            }

            context.ResponseHeaders.ShouldBe(expectedHeaders);
            context.ResponseTrailers.ShouldBe(expectedTrailers);
            context.ResponseStatus.ShouldBe(expectedStatus);
            disposeCounter.ShouldBe(1);
        }

        [Test]
        public void AsyncUnaryCallWaitResponse()
        {
            var disposeCounter = 0;

            var call = new AsyncUnaryCall<Message>(
                GetAsync(new Message()),
                GetNotSupportedAsync<Metadata>(),
                () => throw new NotSupportedException(),
                () => throw new NotSupportedException(),
                () => disposeCounter++);

            using (new SynchronizationContextMock())
            {
                ClientChannelAdapter.AsyncUnaryCallWait(call, null, CancellationToken.None).Wait();
            }

            disposeCounter.ShouldBe(1);
        }

        [Test]
        public void GetAsyncUnaryCallResultResponseHeaders()
        {
            var expectedMessage = new Message<int>(10);
            var expectedHeaders = new Metadata();
            var expectedStatus = new Status(StatusCode.Internal, "some details");
            var expectedTrailers = new Metadata();
            var disposeCounter = 0;

            var context = new CallContext(headers: null);

            var call = new AsyncUnaryCall<Message<int>>(
                GetAsync(expectedMessage),
                GetAsync(expectedHeaders),
                () => expectedStatus,
                () => expectedTrailers,
                () => disposeCounter++);

            using (new SynchronizationContextMock())
            {
                var result = ClientChannelAdapter.GetAsyncUnaryCallResult(call, context, CancellationToken.None).Result;
                result.ShouldBe(10);
            }

            context.ResponseHeaders.ShouldBe(expectedHeaders);
            context.ResponseTrailers.ShouldBe(expectedTrailers);
            context.ResponseStatus.ShouldBe(expectedStatus);
            disposeCounter.ShouldBe(1);
        }

        [Test]
        public void GetAsyncUnaryCallResultResponse()
        {
            var expectedMessage = new Message<int>(10);
            var disposeCounter = 0;

            var context = new CallContext(headers: null);

            var call = new AsyncUnaryCall<Message<int>>(
                GetAsync(expectedMessage),
                GetNotSupportedAsync<Metadata>(),
                () => throw new NotSupportedException(),
                () => throw new NotSupportedException(),
                () => disposeCounter++);

            using (new SynchronizationContextMock())
            {
                var result = ClientChannelAdapter.GetAsyncUnaryCallResult(call, null, CancellationToken.None).Result;
                result.ShouldBe(10);
            }

            disposeCounter.ShouldBe(1);
        }

        private static async Task<T> GetNotSupportedAsync<T>()
        {
            await Task.Delay(100).ConfigureAwait(false);
            throw new NotSupportedException();
        }

        private static async Task<T> GetAsync<T>(T value)
        {
            await Task.Delay(100).ConfigureAwait(false);
            return value;
        }

        private sealed class NotSupportedSynchronizationContext : SynchronizationContext
        {
            public int SendOrPostCounter { get; private set; }

            public override void Post(SendOrPostCallback d, object? state)
            {
                SendOrPostCounter++;
                base.Post(d, state);
            }

            public override void Send(SendOrPostCallback d, object? state)
            {
                SendOrPostCounter++;
                base.Send(d, state);
            }
        }

        private sealed class SynchronizationContextMock : IDisposable
        {
            private readonly SynchronizationContext? _restoreTo;
            private readonly NotSupportedSynchronizationContext _context;

            public SynchronizationContextMock()
            {
                _restoreTo = SynchronizationContext.Current;
                _context = new NotSupportedSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_context);
            }

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(_restoreTo);
                _context.SendOrPostCounter.ShouldBe(0);
            }
        }
    }
}
