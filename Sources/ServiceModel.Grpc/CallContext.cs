using Grpc.Core;

namespace ServiceModel.Grpc
{
    public readonly struct CallContext
    {
        public CallContext(ServerCallContext serverCallContext)
        {
            ServerCallContext = serverCallContext;
            ClientCallContext = default;
        }

        public CallContext(CallOptions clientCallContext)
        {
            ServerCallContext = null;
            ClientCallContext = clientCallContext;
        }

        public ServerCallContext ServerCallContext { get; }

        public CallOptions? ClientCallContext { get; }

        public static implicit operator CallOptions(CallContext context) => context.ClientCallContext.GetValueOrDefault();

        public static implicit operator CallContext(CallOptions clientCallContext) => new CallContext(clientCallContext);

        public static implicit operator CallContext(ServerCallContext serverCallContext) => new CallContext(serverCallContext);
    }
}
