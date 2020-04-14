using System;
using System.Threading;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal ref struct CallOptionsBuilder
    {
        private CallOptions _options;
        private CancellationToken? _token;

        public CallOptionsBuilder(Func<CallOptions> defaultOptionsFactory)
        {
            _options = defaultOptionsFactory?.Invoke() ?? default;
            _token = null;
        }

        public CallOptionsBuilder WithCallOptions(CallOptions options)
        {
            _options = options;
            return this;
        }

        public CallOptionsBuilder WithCancellationToken(CancellationToken token)
        {
            _token = token;
            return this;
        }

        public CallOptionsBuilder WithCallContext(CallContext context)
        {
            if (context.ClientCallContext.HasValue)
            {
                return WithCallOptions(context.ClientCallContext.Value);
            }

            return this;
        }

        public CallOptionsBuilder WithServerCallContext(ServerCallContext context)
        {
            if (context != null)
            {
                throw new NotSupportedException("ServerCallContext cannot be propagated from client call.");
            }

            return this;
        }

        public CallOptions Build()
        {
            var options = _options;
            if (_token.HasValue)
            {
                options = options.WithCancellationToken(_token.Value);
            }

            return options;
        }
    }
}
