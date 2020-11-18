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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Hosting
{
    /// <summary>
    /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
    /// This API may change or be removed in future releases.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServerChannelAdapter
    {
        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="T">Headers container type.</typeparam>
        /// <param name="marshaller"><see cref="Marshaller{T}"/>.</param>
        /// <param name="context"><see cref="ServerCallContext"/>.</param>
        /// <returns>Headers.</returns>
        public static T GetMethodInputHeader<T>(Marshaller<T> marshaller, ServerCallContext context)
        {
            return CompatibilityTools.GetMethodInputFromHeader(marshaller, context.RequestHeaders);
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="stream"><see cref="IAsyncStreamReader{T}"/>.</param>
        /// <param name="context"><see cref="ServerCallContext"/>.</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/>.</returns>
        public static async IAsyncEnumerable<T> ReadClientStream<T>(IAsyncStreamReader<Message<T>> stream, ServerCallContext context)
        {
            while (await stream.MoveNext(context.CancellationToken).ConfigureAwait(false))
            {
                yield return stream.Current.Value1;
            }
        }

        /// <summary>
        /// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="result"><see cref="IAsyncEnumerable{T}"/>.</param>
        /// <param name="stream"><see cref="IServerStreamWriter{T}"/>.</param>
        /// <param name="context"><see cref="ServerCallContext"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        public static async Task WriteServerStreamingResult<T>(IAsyncEnumerable<T> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            await foreach (var i in result.WithCancellation(context.CancellationToken).ConfigureAwait(false))
            {
                await stream.WriteAsync(new Message<T>(i)).ConfigureAwait(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ServerCallContext GetContext(ServerCallContext context) => context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CancellationToken GetContextToken(ServerCallContext context) => context.CancellationToken;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CallContext GetContextDefault(ServerCallContext context) => context;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CallOptions GetContextOptions(ServerCallContext context)
        {
            return new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions);
        }

        internal static async Task<Message> UnaryCallWaitTask(Task call)
        {
            await call;
            return new Message();
        }

        internal static async Task<Message> UnaryCallWaitValueTask(ValueTask call)
        {
            await call;
            return new Message();
        }

        internal static async Task<Message<T>> GetUnaryCallResultTask<T>(Task<T> call)
        {
            var result = await call.ConfigureAwait(false);
            return new Message<T>(result);
        }

        internal static async Task<Message<T>> GetUnaryCallResultValueTask<T>(ValueTask<T> call)
        {
            var result = await call.ConfigureAwait(false);
            return new Message<T>(result);
        }

        internal static async Task WriteServerStreamingResultTask<T>(Task<IAsyncEnumerable<T>> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            var source = await result.ConfigureAwait(false);
            await WriteServerStreamingResult(source, stream, context).ConfigureAwait(false);
        }

        internal static async Task WriteServerStreamingResultValueTask<T>(ValueTask<IAsyncEnumerable<T>> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            var source = await result.ConfigureAwait(false);
            await ServerChannelAdapter.WriteServerStreamingResult<T>(source, stream, context);
        }

        internal static MethodInfo GetServiceContextOptionMethod(Type optionType)
        {
            return typeof(ServerChannelAdapter).StaticMethodByReturnType(nameof(GetContext), optionType);
        }

        internal static bool TryGetServiceContextOptionMethod(Type optionType)
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
    }
}
