// <copyright>
// Copyright Max Ieremenko
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

using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors.Internal;

internal sealed class ClientStreamWriterInterceptor<TRequest> : IClientStreamWriter<TRequest>
{
    private readonly IClientStreamWriter<TRequest> _original;
    private readonly ClientCallInterceptorContext _context;
    private readonly IClientCallInterceptor _interceptor;

    public ClientStreamWriterInterceptor(
        IClientStreamWriter<TRequest> original,
        ClientCallInterceptorContext context,
        IClientCallInterceptor interceptor)
    {
        _original = original;
        _context = context;
        _interceptor = interceptor;
    }

    public WriteOptions? WriteOptions
    {
        get => _original.WriteOptions;
        set => _original.WriteOptions = value;
    }

    public async Task WriteAsync(TRequest message)
    {
        try
        {
            await _original.WriteAsync(message).ConfigureAwait(false);
        }
        catch (RpcException ex)
        {
            _interceptor.OnError(_context, ex);
            throw;
        }
    }

    public async Task CompleteAsync()
    {
        try
        {
            await _original.CompleteAsync().ConfigureAwait(false);
        }
        catch (RpcException ex)
        {
            _interceptor.OnError(_context, ex);
            throw;
        }
    }
}