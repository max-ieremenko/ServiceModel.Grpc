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
using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Represents the pipeline of <see cref="IServerErrorHandler"/>.
/// </summary>
public sealed class ServerErrorHandlerCollection : ServerErrorHandlerBase
{
    private readonly List<IServerErrorHandler> _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerErrorHandlerCollection"/> class.
    /// </summary>
    public ServerErrorHandlerCollection()
    {
        _pipeline = new List<IServerErrorHandler>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerErrorHandlerCollection"/> class with the list of specified <see cref="IServerErrorHandler"/>.
    /// </summary>
    /// <param name="errorHandlers">The list of error handlers.</param>
    public ServerErrorHandlerCollection(params IServerErrorHandler[] errorHandlers)
        : this((IEnumerable<IServerErrorHandler>)errorHandlers)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerErrorHandlerCollection"/> class with the list of specified <see cref="IServerErrorHandler"/>.
    /// </summary>
    /// <param name="errorHandlers">The list of error handlers.</param>
    public ServerErrorHandlerCollection(IEnumerable<IServerErrorHandler> errorHandlers)
    {
        _pipeline = new List<IServerErrorHandler>(GrpcPreconditions.CheckNotNull(errorHandlers, nameof(errorHandlers)));
    }

    /// <summary>
    /// Gets the list of error handlers.
    /// </summary>
    public IReadOnlyList<IServerErrorHandler> Pipeline => _pipeline;

    /// <summary>
    /// Adds the given error handler to the end of this list.
    /// </summary>
    /// <param name="errorHandler">The <see cref="IServerErrorHandler"/>.</param>
    public void Add(IServerErrorHandler errorHandler)
    {
        GrpcPreconditions.CheckNotNull(errorHandler, nameof(errorHandler));

        _pipeline.Add(errorHandler);
    }

    /// <summary>
    /// Invokes ProvideFaultOrIgnore of handlers in the pipeline ony by one until the result is not null.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
    /// <returns>The first non null result provided by a handler in the pipeline, otherwise null.</returns>
    protected override ServerFaultDetail? ProvideFaultOrIgnoreCore(ServerCallInterceptorContext context, Exception error)
    {
        for (var i = 0; i < _pipeline.Count; i++)
        {
            var errorHandler = _pipeline[i];

            var detail = errorHandler.ProvideFaultOrIgnore(context, error);
            if (detail.HasValue)
            {
                return detail;
            }
        }

        return null;
    }
}