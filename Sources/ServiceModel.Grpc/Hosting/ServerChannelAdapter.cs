// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Hosting
{
    internal static class ServerChannelAdapter
    {
        internal static async Task WriteServerStreamingResult<THeader, TResult>(
            ValueTask<(THeader? Header, IAsyncEnumerable<TResult> Response)> result,
            Marshaller<THeader>? headerMarshaller,
            IServerStreamWriter<Message<TResult>> stream,
            ServerCallContext context)
            where THeader : class
        {
            var (header, response) = await result.ConfigureAwait(false);
            if (headerMarshaller != null)
            {
                await context.WriteResponseHeadersAsync(CompatibilityTools.SerializeMethodOutputHeader(headerMarshaller!, header)).ConfigureAwait(false);
            }

            await foreach (var i in response.WithCancellation(context.CancellationToken).ConfigureAwait(false))
            {
                await stream.WriteAsync(new Message<TResult>(i)).ConfigureAwait(false);
            }
        }

        internal static async IAsyncEnumerable<T> ReadClientStream<T>(IAsyncStreamReader<Message<T>> stream, ServerCallContext context)
        {
            while (await stream.MoveNext(context.CancellationToken).ConfigureAwait(false))
            {
                yield return stream.Current.Value1;
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
            await call.ConfigureAwait(false);
            return new Message();
        }

        internal static async Task<Message> UnaryCallWaitValueTask(ValueTask call)
        {
            await call.ConfigureAwait(false);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueTask<(Message? Header, IAsyncEnumerable<TResponse> Stream)> ServerStreaming<TResponse>(IAsyncEnumerable<TResponse> response)
        {
            return new ValueTask<(Message?, IAsyncEnumerable<TResponse>)>((null, response));
        }

        internal static async ValueTask<(Message? Header, IAsyncEnumerable<TResponse> Stream)> ServerStreamingTask<TResponse>(Task<IAsyncEnumerable<TResponse>> responseTask)
        {
            var response = await responseTask.ConfigureAwait(false);
            return (null, response);
        }

        internal static async ValueTask<(Message? Header, IAsyncEnumerable<TResponse> Stream)> ServerStreamingValueTask<TResponse>(ValueTask<IAsyncEnumerable<TResponse>> responseTask)
        {
            var response = await responseTask.ConfigureAwait(false);
            return (null, response);
        }

        internal static async ValueTask<(TResponseHeader Header, IAsyncEnumerable<TResponse> Stream)> ServerStreamingHeaderTask<TResult, TResponseHeader, TResponse>(
            Task<TResult> resultTask,
            Func<TResult, (TResponseHeader Header, IAsyncEnumerable<TResponse> Stream)> adapter)
        {
            var result = await resultTask.ConfigureAwait(false);
            return adapter(result);
        }

        internal static async ValueTask<(TResponseHeader Header, IAsyncEnumerable<TResponse> Stream)> ServerStreamingHeaderValueTask<TResult, TResponseHeader, TResponse>(
            ValueTask<TResult> resultTask,
            Func<TResult, (TResponseHeader Header, IAsyncEnumerable<TResponse> Stream)> adapter)
        {
            var result = await resultTask.ConfigureAwait(false);
            return adapter(result);
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
