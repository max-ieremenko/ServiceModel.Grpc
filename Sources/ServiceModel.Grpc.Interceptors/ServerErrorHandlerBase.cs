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

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Provides basic functionality for server side error handling.
/// </summary>
public abstract class ServerErrorHandlerBase : IServerErrorHandler
{
    /// <summary>
    /// Return true is the current call was cancelled.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
    /// <returns>True if current call was cancelled, otherwise false.</returns>
    public static bool IsOperationCancelled(ServerCallContext context, Exception error)
    {
        return context.CancellationToken.IsCancellationRequested;
    }

    /// <summary>
    /// If current call is cancelled invokes OnOperationCancelled, otherwise invokes ProvideFaultOrIgnoreCore.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
    /// <returns>Result of ProvideFaultOrIgnoreCore if operation is not cancelled, otherwise null.</returns>
    public virtual ServerFaultDetail? ProvideFaultOrIgnore(ServerCallInterceptorContext context, Exception error)
    {
        // call is already aborted by client, no reason to provide details
        if (IsOperationCancelled(context.ServerCallContext, error))
        {
            OnOperationCancelled(context, error);
            return null;
        }

        return ProvideFaultOrIgnoreCore(context, error);
    }

    /// <summary>
    /// Enables custom action when call is cancelled.
    /// Call is already aborted by a client, no reason to provide details.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
    protected virtual void OnOperationCancelled(ServerCallInterceptorContext context, Exception error)
    {
    }

    /// <summary>
    /// Allows to provide external detail to a client call.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
    /// <returns>Optional error detail that is returned to the client.</returns>
    protected abstract ServerFaultDetail? ProvideFaultOrIgnoreCore(ServerCallInterceptorContext context, Exception error);
}