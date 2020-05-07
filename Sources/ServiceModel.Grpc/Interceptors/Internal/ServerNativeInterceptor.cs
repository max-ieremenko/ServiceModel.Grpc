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
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ServiceModel.Grpc.Interceptors.Internal
{
    internal sealed class ServerNativeInterceptor : Interceptor
    {
        private readonly IServerCallInterceptor _interceptor;

        public ServerNativeInterceptor(IServerCallInterceptor interceptor)
        {
            interceptor.AssertNotNull(nameof(interceptor));

            _interceptor = interceptor;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await base.UnaryServerHandler(request, context, continuation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _interceptor.OnError(new ServerCallInterceptorContext(context), ex);
                throw;
            }
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await base.ClientStreamingServerHandler(requestStream, context, continuation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _interceptor.OnError(new ServerCallInterceptorContext(context), ex);
                throw;
            }
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                await base.ServerStreamingServerHandler(request, responseStream, context, continuation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _interceptor.OnError(new ServerCallInterceptorContext(context), ex);
                throw;
            }
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                await base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _interceptor.OnError(new ServerCallInterceptorContext(context), ex);
                throw;
            }
        }
    }
}
