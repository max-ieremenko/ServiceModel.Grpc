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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModel.Grpc.TestApi.Domain
{
    public sealed class ErrorService : IErrorService
    {
        public void ThrowApplicationException(string message)
        {
            throw new ApplicationException(message);
        }

        public Task ThrowApplicationExceptionAsync(string message)
        {
            throw new ApplicationException(message);
        }

        public void ThrowOperationCanceledException()
        {
            throw new OperationCanceledException();
        }

        public void PassSerializationFail(DomainObjectSerializationFail fail)
        {
        }

        public DomainObjectSerializationFail ReturnSerializationFail(string? onDeserializedError, string? onSerializedError)
        {
            return new DomainObjectSerializationFail
            {
                OnDeserializedError = onDeserializedError,
                OnSerializedError = onSerializedError
            };
        }

        public void CancelOperation(CancellationToken token)
        {
            WaitForCancel(token);
            token.ThrowIfCancellationRequested();
        }

        public async Task CancelOperationAsync(CancellationToken token)
        {
            await WaitForCancelAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
        }

        public void ThrowApplicationExceptionAfterCancel(string message, CancellationToken token)
        {
            WaitForCancel(token);
            throw new ApplicationException(message);
        }

        public async Task ThrowApplicationExceptionAfterCancelAsync(string message, CancellationToken token)
        {
            await WaitForCancelAsync(token).ConfigureAwait(false);
            throw new ApplicationException(message);
        }

        public async Task ThrowApplicationExceptionClientStreaming(IAsyncEnumerable<int> data, int readsCount, string message, CallContext context)
        {
            if (readsCount == 0)
            {
                throw new ApplicationException(message);
            }

            var counter = 0;
            await foreach (var i in data.ConfigureAwait(false))
            {
                counter++;
                if (counter == readsCount)
                {
                    throw new ApplicationException(message);
                }
            }

            throw new ApplicationException(message);
        }

        public async Task<IAsyncEnumerable<int>> ThrowApplicationExceptionServerStreaming(int writesCount, string message)
        {
            if (writesCount == 0)
            {
                throw new ApplicationException(message);
            }

            var channel = System.Threading.Channels.Channel.CreateUnbounded<int>();
            await channel.Writer.WriteAsync(1).ConfigureAwait(false);

            if (writesCount > 1)
            {
                throw new ApplicationException(message);
            }

            channel.Writer.Complete(new ApplicationException(message));
            return channel.Reader.AsAsyncEnumerable(CancellationToken.None);
        }

        public ValueTask<(IAsyncEnumerable<int> Stream, string Message)> ThrowApplicationExceptionServerStreamingHeader(string message)
        {
            throw new ApplicationException(message);
        }

        public async IAsyncEnumerable<int> ThrowApplicationExceptionDuplexStreaming(IAsyncEnumerable<int> data, string message, int readsCount, CallContext context)
        {
            if (readsCount == 0)
            {
                throw new ApplicationException(message);
            }

            var counter = 0;
            await foreach (var i in data.ConfigureAwait(false))
            {
                counter++;
                yield return i;

                if (counter == readsCount)
                {
                    throw new ApplicationException(message);
                }
            }

            throw new ApplicationException(message);
        }

        public async Task<(IAsyncEnumerable<int> Stream, string Message)> ThrowApplicationExceptionDuplexStreamingHeader(IAsyncEnumerable<int> data, string message, int readsCount, CallContext context)
        {
            if (readsCount == 0)
            {
                throw new ApplicationException(message);
            }

            var counter = 0;
            await foreach (var i in data.ConfigureAwait(false))
            {
                counter++;

                if (counter == readsCount)
                {
                    throw new ApplicationException(message);
                }
            }

            throw new ApplicationException(message);
        }

        private static void WaitForCancel(CancellationToken token)
        {
            var timeout = Stopwatch.StartNew();
            while (!token.IsCancellationRequested && timeout.Elapsed < TimeSpan.FromSeconds(10))
            {
                Thread.Sleep(300);
            }
        }

        private static async Task WaitForCancelAsync(CancellationToken token)
        {
            var timeout = Stopwatch.StartNew();
            while (!token.IsCancellationRequested && timeout.Elapsed < TimeSpan.FromSeconds(10))
            {
                await Task.Delay(300, token).ConfigureAwait(false);
            }
        }
    }
}
