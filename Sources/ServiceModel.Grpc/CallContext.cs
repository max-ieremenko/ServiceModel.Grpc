using System;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace ServiceModel.Grpc
{
    public sealed class CallContext
    {
        internal const string HeaderNameMethodInput = "smgrpc-method-input-bin";

        public CallContext(
            Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
            : this(new CallOptions(headers: headers, deadline: deadline, cancellationToken: cancellationToken))
        {
        }

        public CallContext(CallOptions callOptions)
        {
            CallOptions = callOptions;
        }

        public CallContext(ServerCallContext serverCallContext)
        {
            ServerCallContext = serverCallContext;
        }

        public ServerCallContext ServerCallContext { get; }

        public CallOptions? CallOptions { get; }

        public Status? ResponseStatus => ServerResponse?.ResponseStatus;

        public Metadata ResponseHeaders => ServerResponse?.ResponseHeaders;

        public Metadata ResponseTrailers => ServerResponse?.ResponseTrailers;

        internal ServerResponse? ServerResponse { get; set; }

        public static implicit operator CallOptions(CallContext context) => context.CallOptions.GetValueOrDefault();

        public static implicit operator CallContext(CallOptions clientCallContext) => new CallContext(clientCallContext);

        public static implicit operator CallContext(ServerCallContext serverCallContext) => new CallContext(serverCallContext);

        public static implicit operator ServerCallContext(CallContext context) => context.ServerCallContext;
    }
}
