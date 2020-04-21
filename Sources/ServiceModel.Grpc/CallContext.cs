using System;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc
{
    /// <summary>
    /// Abstraction to unify client and sever calls.
    /// </summary>
    public sealed class CallContext
    {
        internal const string HeaderNameMethodInput = "smgrpc-method-input-bin";

        /// <summary>
        /// Initializes a new instance of the <see cref="CallContext"/> class for a client call.
        /// </summary>
        /// <param name="headers">Headers to be sent with the call.</param>
        /// <param name="deadline">Deadline for the call to finish. null means no deadline.</param>
        /// <param name="cancellationToken">Can be used to request cancellation of the call.</param>
        public CallContext(
            Metadata headers = null,
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
            ServerCallContext = serverCallContext;
        }

        /// <summary>
        /// Gets the context for a server-side call. Always null for client-side.
        /// </summary>
        public ServerCallContext ServerCallContext { get; }

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
        public Metadata ResponseHeaders => ServerResponse?.ResponseHeaders;

        /// <summary>
        /// Gets the client call trailing metadata if the call has already finished. Available only for a client call.
        /// </summary>
        public Metadata ResponseTrailers => ServerResponse?.ResponseTrailers;

        internal ServerResponse? ServerResponse { get; set; }

        /// <summary>
        /// Crates <see cref="CallOptions"/> based on <see cref="CallContext"/>.CallOptions.
        /// </summary>
        /// <param name="context">The context.</param>
        public static implicit operator CallOptions(CallContext context) => context.CallOptions.GetValueOrDefault();

        /// <summary>
        /// Crates <see cref="CallContext"/> with CallOptions based on <see cref="CallOptions"/>.
        /// </summary>
        /// <param name="clientCallContext">The context.</param>
        public static implicit operator CallContext(CallOptions clientCallContext) => new CallContext(clientCallContext);

        /// <summary>
        /// Crates <see cref="CallContext"/> with ServerCallContext based on <see cref="ServerCallContext"/>.
        /// </summary>
        /// <param name="serverCallContext">The context.</param>
        public static implicit operator CallContext(ServerCallContext serverCallContext) => new CallContext(serverCallContext);

        /// <summary>
        /// Crates <see cref="ServerCallContext"/> based on <see cref="CallContext"/>.ServerCallContext.
        /// </summary>
        /// <param name="context">The context.</param>
        public static implicit operator ServerCallContext(CallContext context) => context.ServerCallContext;
    }
}
