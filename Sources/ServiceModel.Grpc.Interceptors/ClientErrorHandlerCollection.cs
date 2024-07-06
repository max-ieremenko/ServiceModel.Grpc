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

using Grpc.Core.Utils;

namespace ServiceModel.Grpc.Interceptors;

/// <summary>
/// Represents the pipeline of <see cref="IClientErrorHandler"/>.
/// </summary>
public sealed class ClientErrorHandlerCollection : ClientErrorHandlerBase
{
    private readonly List<IClientErrorHandler> _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientErrorHandlerCollection"/> class.
    /// </summary>
    public ClientErrorHandlerCollection()
    {
        _pipeline = new List<IClientErrorHandler>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientErrorHandlerCollection"/> class with the list of specified <see cref="IClientErrorHandler"/>.
    /// </summary>
    /// <param name="errorHandlers">The list of error handlers.</param>
    public ClientErrorHandlerCollection(params IClientErrorHandler[] errorHandlers)
        : this((IEnumerable<IClientErrorHandler>)errorHandlers)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientErrorHandlerCollection"/> class with the list of specified <see cref="IClientErrorHandler"/>.
    /// </summary>
    /// <param name="errorHandlers">The list of error handlers.</param>
    public ClientErrorHandlerCollection(IEnumerable<IClientErrorHandler> errorHandlers)
    {
        _pipeline = new List<IClientErrorHandler>(GrpcPreconditions.CheckNotNull(errorHandlers, nameof(errorHandlers)));
    }

    /// <summary>
    /// Gets the list of error handlers.
    /// </summary>
    public IReadOnlyList<IClientErrorHandler> Pipeline => _pipeline;

    /// <summary>
    /// Adds the given error handler to the end of this list.
    /// </summary>
    /// <param name="errorHandler">The <see cref="IServerErrorHandler"/>.</param>
    public void Add(IClientErrorHandler errorHandler)
    {
        GrpcPreconditions.CheckNotNull(errorHandler, nameof(errorHandler));

        _pipeline.Add(errorHandler);
    }

    /// <summary>
    /// Invokes ThrowOrIgnore of handlers in the pipeline ony by one.
    /// </summary>
    /// <param name="context">The context associated with the current call.</param>
    /// <param name="detail">The exception details.</param>
    protected override void ThrowOrIgnoreCore(ClientCallInterceptorContext context, ClientFaultDetail detail)
    {
        for (var i = 0; i < _pipeline.Count; i++)
        {
            var handler = _pipeline[i];

            handler.ThrowOrIgnore(context, detail);
        }
    }
}