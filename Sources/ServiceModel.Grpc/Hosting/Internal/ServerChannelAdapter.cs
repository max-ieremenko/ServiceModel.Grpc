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
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Emit;

namespace ServiceModel.Grpc.Hosting.Internal;

internal static class ServerChannelAdapter
{
    internal static async Task WriteServerStreamingResult<THeader, TMessage, TValue>(
        ValueTask<(THeader? Header, IAsyncEnumerable<TValue?> Response)> result,
        Marshaller<THeader>? headerMarshaller,
        IServerStreamWriter<TMessage> stream,
        ServerCallContext context)
        where THeader : class
        where TMessage : class, IMessage<TValue>, new()
    {
        var (header, response) = await result.ConfigureAwait(false);
        if (headerMarshaller != null)
        {
            await context.WriteResponseHeadersAsync(CompatibilityTools.SerializeMethodOutputHeader(headerMarshaller!, header)).ConfigureAwait(false);
        }

        var token = context.CancellationToken;
        await foreach (var i in response.WithCancellation(token).ConfigureAwait(false))
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var message = new TMessage();
                message.SetValue1(i);
                await stream.WriteAsync(message).ConfigureAwait(false);
            }
            catch (InvalidOperationException) when (token.IsCancellationRequested)
            {
                // asp.net host InvalidOperationException: Can't write the message because the request is complete.
                // self host InvalidOperationException: Already finished.
                // one of the reasons the client does not read the whole response, see test MultipurposeServiceTestBase.ClientStreamingStopReading
                break;
            }
            catch (IOException) when (token.IsCancellationRequested)
            {
                // self host IOException: Error sending from server.
                // one of the reasons the client does not read the whole response, see test MultipurposeServiceTestBase.DuplexStreamingClientStopReading
                break;
            }
        }
    }

    internal static async IAsyncEnumerable<TValue?> ReadClientStream<TMessage, TValue>(IAsyncStreamReader<TMessage> stream, ServerCallContext context)
        where TMessage : class, IMessage<TValue>
    {
        // in case of client does not read the whole response
        // TaskCanceledException A task was canceled, test MultipurposeServiceTestBase.see DuplexStreamingClientStopReading.
        // Do not catch by purpose: exception must be handled by server method implementation.
        // If the exception was ignored by server method implementation, then it will be re-throw in WriteServerStreamingResult on foreach.
        while (await stream.MoveNext(context.CancellationToken).ConfigureAwait(false))
        {
            yield return stream.Current.GetValue1();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ServerCallContext GetContext(ServerCallContext context) => context;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CancellationToken GetContextToken(ServerCallContext context) => context.CancellationToken;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CancellationToken? GetContextNullableToken(ServerCallContext context) => GetContextToken(context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CallContext GetContextDefault(ServerCallContext context) => context;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CallOptions GetContextOptions(ServerCallContext context)
    {
        return new CallOptions(context.RequestHeaders, context.Deadline, context.CancellationToken, context.WriteOptions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CallOptions? GetContextNullableOptions(ServerCallContext context) => GetContextOptions(context);

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
    internal static ValueTask<(Message? Header, IAsyncEnumerable<TResponse?> Stream)> ServerStreaming<TResponse>(IAsyncEnumerable<TResponse?> response) =>
        new((null, response));

    internal static async ValueTask<(Message? Header, IAsyncEnumerable<TResponse?> Stream)> ServerStreamingTask<TResponse>(Task<IAsyncEnumerable<TResponse?>> responseTask)
    {
        var response = await responseTask.ConfigureAwait(false);
        return (null, response);
    }

    internal static async ValueTask<(Message? Header, IAsyncEnumerable<TResponse?> Stream)> ServerStreamingValueTask<TResponse>(ValueTask<IAsyncEnumerable<TResponse?>> responseTask)
    {
        var response = await responseTask.ConfigureAwait(false);
        return (null, response);
    }

    internal static async ValueTask<(TResponseHeader Header, IAsyncEnumerable<TResponse?> Stream)> ServerStreamingHeaderTask<TResult, TResponseHeader, TResponse>(
        Task<TResult> resultTask,
        Func<TResult, (TResponseHeader Header, IAsyncEnumerable<TResponse?> Stream)> adapter)
    {
        var result = await resultTask.ConfigureAwait(false);
        return adapter(result);
    }

    internal static async ValueTask<(TResponseHeader Header, IAsyncEnumerable<TResponse?> Stream)> ServerStreamingHeaderValueTask<TResult, TResponseHeader, TResponse>(
        ValueTask<TResult> resultTask,
        Func<TResult, (TResponseHeader Header, IAsyncEnumerable<TResponse?> Stream)> adapter)
    {
        var result = await resultTask.ConfigureAwait(false);
        return adapter(result);
    }

    internal static MethodInfo GetServiceContextOptionMethod(Type optionType) =>
        typeof(ServerChannelAdapter).StaticMethodByReturnType(nameof(GetContext), optionType);

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