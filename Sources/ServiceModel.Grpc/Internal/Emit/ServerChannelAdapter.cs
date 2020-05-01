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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class ServerChannelAdapter
    {
        public static MethodInfo GetServiceContextOptionMethod(Type optionType)
        {
            return typeof(ServerChannelAdapter).StaticMethodByReturnType(nameof(GetContext), optionType);
        }

        public static bool TryGetServiceContextOptionMethod(Type optionType)
        {
            try
            {
                GetServiceContextOptionMethod(optionType);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                // method not found
            }

            return false;
        }

        public static ServerCallContext GetContext(ServerCallContext context) => context;

        public static CancellationToken GetContextToken(ServerCallContext context) => context.CancellationToken;

        public static CallContext GetContextDefault(ServerCallContext context) => context;

        public static CallOptions GetContextOptions(ServerCallContext context)
        {
            return new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions);
        }

        public static T GetMethodInputHeader<T>(Marshaller<T> marshaller, ServerCallContext context)
        {
            return CompatibilityTools.GetMethodInputFromHeader(marshaller, context.RequestHeaders);
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
