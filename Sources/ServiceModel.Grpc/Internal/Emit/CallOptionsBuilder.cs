using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal ref struct CallOptionsBuilder
    {
        private CallOptions _options;

        public CallOptionsBuilder(Func<CallOptions> defaultOptionsFactory)
        {
            _options = defaultOptionsFactory?.Invoke() ?? default;
        }

        public CallOptionsBuilder WithCallOptions(CallOptions options)
        {
            _options = MergeCallOptions(_options, options);
            return this;
        }

        public CallOptionsBuilder WithCancellationToken(CancellationToken token)
        {
            _options = MergeCallOptions(_options, new CallOptions(cancellationToken: token));
            return this;
        }

        public CallOptionsBuilder WithCallContext(CallContext context)
        {
            var options = context?.CallOptions;
            if (options.HasValue)
            {
                return WithCallOptions(options.Value);
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

        public CallOptionsBuilder WithMethodInputHeader<T>(Marshaller<T> marshaller, T value)
        {
            return WithCallOptions(new CallOptions(new Metadata
            {
                { CallContext.HeaderNameMethodInput, marshaller.Serializer(value) }
            }));
        }

        public CallOptions Build() => _options;

        internal static Metadata MergeMetadata(Metadata current, Metadata mergeWith)
        {
            if (current == null)
            {
                return mergeWith;
            }

            if (mergeWith == null)
            {
                return current;
            }

            var result = new Metadata();
            foreach (var entry in mergeWith)
            {
                result.Add(entry);
            }

            foreach (var entry in current)
            {
                var exists = result.Any(i => HeadersAreEqual(i, entry));
                if (!exists)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private static CallOptions MergeCallOptions(CallOptions current, CallOptions mergeWith)
        {
            return new CallOptions(
                headers: MergeMetadata(current.Headers, mergeWith.Headers),
                deadline: mergeWith.Deadline ?? current.Deadline,
                cancellationToken: MergeToken(current.CancellationToken, mergeWith.CancellationToken),
                writeOptions: mergeWith.WriteOptions ?? current.WriteOptions,
                propagationToken: mergeWith.PropagationToken ?? current.PropagationToken,
                credentials: mergeWith.Credentials ?? current.Credentials);
        }

        private static bool HeadersAreEqual(Metadata.Entry x, Metadata.Entry y)
        {
            if (!string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase)
                || x.IsBinary != y.IsBinary)
            {
                return false;
            }

            if (x.IsBinary)
            {
                return x.ValueBytes.SequenceEqual(y.ValueBytes);
            }

            return string.Equals(x.Value, y.Value, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CancellationToken MergeToken(CancellationToken current, CancellationToken mergeWith)
        {
            if (!mergeWith.CanBeCanceled || mergeWith.Equals(current))
            {
                return current;
            }

            if (!current.CanBeCanceled)
            {
                return mergeWith;
            }

            throw new NotSupportedException("Too many cancellation tokens.");
        }
    }
}
