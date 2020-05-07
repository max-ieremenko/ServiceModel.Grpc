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
using Grpc.Core;

namespace ServiceModel.Grpc.Interceptors
{
    /// <summary>
    /// Provides basic functionality for client side error handling.
    /// </summary>
    public abstract class ClientErrorHandlerBase : IClientErrorHandler
    {
        /// <summary>
        /// Return true is the current call was cancelled.
        /// </summary>
        /// <param name="context">The context associated with the current call.</param>
        /// <param name="error">The original <see cref="RpcException"/> raised by <see cref="CallInvoker"/>.</param>
        /// <returns>True if current call was cancelled, otherwise false.</returns>
        public static bool IsOperationCancelled(ClientCallInterceptorContext context, RpcException error)
        {
            return context.CallOptions.CancellationToken.IsCancellationRequested;
        }

        /// <summary>
        /// If current call is cancelled invokes OnOperationCancelled, otherwise invokes ThrowOrIgnoreCore.
        /// </summary>
        /// <param name="context">The context associated with the current call.</param>
        /// <param name="detail">The exception details.</param>
        public virtual void ThrowOrIgnore(ClientCallInterceptorContext context, ClientFaultDetail detail)
        {
            // the request is already aborted, no extra details are available from server
            if (IsOperationCancelled(context, detail.OriginalError))
            {
                OnOperationCancelled(context, detail.OriginalError);
            }

            ThrowOrIgnoreCore(context, detail);
        }

        /// <summary>
        /// Enables custom action when call is cancelled.
        /// Call is already aborted by a client, no external detail is available.
        /// </summary>
        /// <param name="context">The context associated with the current call.</param>
        /// <param name="error">The original <see cref="RpcException"/> raised by <see cref="CallInvoker"/>.</param>
        protected virtual void OnOperationCancelled(ClientCallInterceptorContext context, RpcException error)
        {
            throw new OperationCanceledException(null, error, context.CallOptions.CancellationToken);
        }

        /// <summary>
        /// Handle the exception that was raised by <see cref="CallInvoker"/>.
        /// </summary>
        /// <param name="context">The context associated with the current call.</param>
        /// <param name="detail">The exception details.</param>
        protected abstract void ThrowOrIgnoreCore(ClientCallInterceptorContext context, ClientFaultDetail detail);
    }
}
