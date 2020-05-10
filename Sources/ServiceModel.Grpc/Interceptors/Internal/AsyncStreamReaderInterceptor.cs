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

using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors.Internal
{
    internal sealed class AsyncStreamReaderInterceptor<TResponse> : IAsyncStreamReader<TResponse>
    {
        private readonly IAsyncStreamReader<TResponse> _original;
        private readonly ClientCallInterceptorContext _context;
        private readonly IClientCallInterceptor _interceptor;

        public AsyncStreamReaderInterceptor(
            IAsyncStreamReader<TResponse> original,
            ClientCallInterceptorContext context,
            IClientCallInterceptor interceptor)
        {
            _original = original;
            _context = context;
            _interceptor = interceptor;
        }

        public TResponse Current => _original.Current;

        public async Task<bool> MoveNext(CancellationToken token)
        {
            try
            {
                return await _original.MoveNext(token).ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                _interceptor.OnError(_context, ex);
                throw;
            }
        }
    }
}
