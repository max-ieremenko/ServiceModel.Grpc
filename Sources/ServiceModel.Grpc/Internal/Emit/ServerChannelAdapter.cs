using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ServerChannelAdapter
    {
        public static ServerCallContext GetContext(ServerCallContext context) => context;

        public static CancellationToken GetContextToken(ServerCallContext context) => context.CancellationToken;

        public static CallContext GetContextDefault(ServerCallContext context) => context;

        public static CallOptions GetContextOptions(ServerCallContext context)
        {
            return new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions);
        }

        public static T GetMethodInputHeader<T>(Marshaller<T> marshaller, ServerCallContext context)
        {
            var header = context.RequestHeaders?.FirstOrDefault(i => i.IsBinary && CallContext.HeaderNameMethodInput.Equals(i.Key, StringComparison.OrdinalIgnoreCase));
            if (header == null)
            {
                throw new InvalidOperationException("Fail to resolve header parameters, {0} header not found.".FormatWith(CallContext.HeaderNameMethodInput));
            }

            return marshaller.Deserializer(header.ValueBytes);
        }

        public static async Task<Message> UnaryCallWaitTask(Task call)
        {
            await call;
            return new Message();
        }

        public static async Task<Message> UnaryCallWaitValueTask(ValueTask call)
        {
            await call;
            return new Message();
        }

        public static async Task<Message<T>> GetUnaryCallResultTask<T>(Task<T> call)
        {
            var result = await call.ConfigureAwait(false);
            return new Message<T>(result);
        }

        public static async Task<Message<T>> GetUnaryCallResultValueTask<T>(ValueTask<T> call)
        {
            var result = await call.ConfigureAwait(false);
            return new Message<T>(result);
        }

        public static async Task WriteServerStreamingResult<T>(IAsyncEnumerable<T> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            await foreach (var i in result.WithCancellation(context.CancellationToken))
            {
                await stream.WriteAsync(new Message<T>(i)).ConfigureAwait(false);
            }
        }

        public static async IAsyncEnumerable<T> ReadClientStream<T>(IAsyncStreamReader<Message<T>> stream, ServerCallContext context)
        {
            while (await stream.MoveNext(context.CancellationToken).ConfigureAwait(false))
            {
                yield return stream.Current.Value1;
            }
        }
    }
}
