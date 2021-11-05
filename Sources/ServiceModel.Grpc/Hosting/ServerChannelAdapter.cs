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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented

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
        /// <exclude />
        public static T GetMethodInputHeader<T>(Marshaller<T> marshaller, ServerCallContext context)
        {
            return CompatibilityTools.DeserializeMethodInputHeader(marshaller, context.RequestHeaders);
        }

        /// <exclude />
        public static async IAsyncEnumerable<T> ReadClientStream<T>(IAsyncStreamReader<Message<T>> stream, ServerCallContext context)
        {
            while (await stream.MoveNext(context.CancellationToken).ConfigureAwait(false))
            {
                yield return stream.Current.Value1;
            }
        }

        /// <exclude />
        public static async Task WriteServerStreamingResult<T>(IAsyncEnumerable<T> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            await foreach (var i in result.WithCancellation(context.CancellationToken).ConfigureAwait(false))
            {
                await stream.WriteAsync(new Message<T>(i)).ConfigureAwait(false);
            }
        }

        /// <exclude />
        public static async Task WriteServerStreamingResult<THeader, TResult>(
            IAsyncEnumerable<TResult> result,
            Marshaller<THeader> headerMarshaller,
            THeader header,
            IServerStreamWriter<Message<TResult>> stream,
            ServerCallContext context)
        {
            await context.WriteResponseHeadersAsync(CompatibilityTools.SerializeMethodOutputHeader(headerMarshaller, header)).ConfigureAwait(false);
            await WriteServerStreamingResult(result, stream, context).ConfigureAwait(false);
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

        internal static async Task WriteServerStreamingResultTask<T>(Task<IAsyncEnumerable<T>> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            var source = await result.ConfigureAwait(false);
            await WriteServerStreamingResult(source, stream, context).ConfigureAwait(false);
        }

        internal static async Task WriteServerStreamingResultValueTask<T>(ValueTask<IAsyncEnumerable<T>> result, IServerStreamWriter<Message<T>> stream, ServerCallContext context)
        {
            var source = await result.ConfigureAwait(false);
            await WriteServerStreamingResult(source, stream, context).ConfigureAwait(false);
        }

        internal static async Task WriteServerStreamingResultWithHeaderTask<TResponse, THeader, TResult>(
            Task<TResponse> responseTask,
            Func<TResponse, (THeader Header, IAsyncEnumerable<TResult> Stream)> convertResponse,
            Marshaller<THeader> headerMarshaller,
            IServerStreamWriter<Message<TResult>> stream,
            ServerCallContext context)
        {
            var response = await responseTask.ConfigureAwait(false);
            var (header, result) = convertResponse(response);

            await context.WriteResponseHeadersAsync(CompatibilityTools.SerializeMethodOutputHeader(headerMarshaller, header)).ConfigureAwait(false);
            await WriteServerStreamingResult(result, stream, context).ConfigureAwait(false);
        }

        internal static async Task WriteServerStreamingResultWithHeaderValueTask<TResponse, THeader, TResult>(
            ValueTask<TResponse> responseTask,
            Func<TResponse, (THeader Header, IAsyncEnumerable<TResult> Stream)> convertResponse,
            Marshaller<THeader> headerMarshaller,
            IServerStreamWriter<Message<TResult>> stream,
            ServerCallContext context)
        {
            var response = await responseTask.ConfigureAwait(false);
            var (header, result) = convertResponse(response);

            await context.WriteResponseHeadersAsync(CompatibilityTools.SerializeMethodOutputHeader(headerMarshaller, header)).ConfigureAwait(false);
            await WriteServerStreamingResult(result, stream, context).ConfigureAwait(false);
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
