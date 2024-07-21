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
using Grpc.Core.Utils;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc;

/// <summary>
/// Abstraction to unify client and sever calls.
/// </summary>
public sealed class CallContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallContext"/> class for a client call.
    /// </summary>
    /// <param name="headers">Headers to be sent with the call.</param>
    /// <param name="deadline">Deadline for the call to finish. null means no deadline.</param>
    /// <param name="cancellationToken">Can be used to request cancellation of the call.</param>
    public CallContext(
        Metadata? headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        : this(new CallOptions(headers: headers, deadline: deadline, cancellationToken: cancellationToken))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CallContext"/> class for a client call.
    /// </summary>
    /// <param name="callOptions">gRPC options for calls made by client.</param>
    public CallContext(CallOptions callOptions)
    {
        CallOptions = callOptions;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CallContext"/> class for a server call handler.
    /// </summary>
    /// <param name="serverCallContext">The context for a server-side call.</param>
    public CallContext(ServerCallContext serverCallContext)
    {
        ServerCallContext = GrpcPreconditions.CheckNotNull(serverCallContext, nameof(serverCallContext));
    }

    /// <summary>
    /// Gets the context for a server-side call. Always null for client-side.
    /// </summary>
    public ServerCallContext? ServerCallContext { get; }

    /// <summary>
    /// Gets the context for a client-side call. Always null for server-side.
    /// </summary>
    public CallOptions? CallOptions { get; }

    /// <summary>
    /// Gets the client call status if the call has already finished. Available only for a client call.
    /// </summary>
    public Status? ResponseStatus => ServerResponse?.ResponseStatus;

    /// <summary>
    /// Gets access to server response headers. Available only for a client call.
    /// </summary>
    public Metadata? ResponseHeaders => ServerResponse?.ResponseHeaders;

    /// <summary>
    /// Gets the client call trailing metadata if the call has already finished. Available only for a client call.
    /// </summary>
    public Metadata? ResponseTrailers => ServerResponse?.ResponseTrailers;

    internal ServerResponse? ServerResponse { get; set; }

    // only for tests
    internal Action<Task>? TraceClientStreaming { get; set; }

    /// <summary>
    /// Crates <see cref="CallOptions"/> based on <see cref="CallContext"/>.CallOptions.
    /// </summary>
    /// <param name="context">The context.</param>
    public static implicit operator CallOptions(CallContext context) => context.CallOptions.GetValueOrDefault();

    /// <summary>
    /// Crates <see cref="CallContext"/> with CallOptions based on <see cref="CallOptions"/>.
    /// </summary>
    /// <param name="clientCallContext">The context.</param>
    public static implicit operator CallContext(CallOptions clientCallContext) => new(clientCallContext);

    /// <summary>
    /// Crates <see cref="CallContext"/> with ServerCallContext based on <see cref="ServerCallContext"/>.
    /// </summary>
    /// <param name="serverCallContext">The context.</param>
    public static implicit operator CallContext(ServerCallContext serverCallContext) => new(serverCallContext);

    /// <summary>
    /// Crates <see cref="ServerCallContext"/> based on <see cref="CallContext"/>.ServerCallContext.
    /// </summary>
    /// <param name="context">The context.</param>
    public static implicit operator ServerCallContext?(CallContext context) => context.ServerCallContext;
}