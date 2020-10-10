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
            await WaitForCancelAsync(token);
            token.ThrowIfCancellationRequested();
        }

        public void ThrowApplicationExceptionAfterCancel(string message, CancellationToken token)
        {
            WaitForCancel(token);
            throw new ApplicationException(message);
        }

        public async Task ThrowApplicationExceptionAfterCancelAsync(string message, CancellationToken token)
        {
            await WaitForCancelAsync(token);
            throw new ApplicationException(message);
        }

        public async Task ThrowApplicationExceptionClientStreaming(IAsyncEnumerable<int> data, string message)
        {
            await foreach (var i in data)
            {
                throw new ApplicationException(message);
            }
        }

        public IAsyncEnumerable<int> ThrowApplicationExceptionServerStreaming(string message)
        {
            throw new ApplicationException(message);
        }

        public async IAsyncEnumerable<int> ThrowApplicationExceptionDuplexStreaming(IAsyncEnumerable<int> data, string message)
        {
            await foreach (var i in data)
            {
                yield return i;
                throw new ApplicationException(message);
            }
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
                await Task.Delay(300, token);
            }
        }
    }
}
