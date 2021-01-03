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
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal
{
    public partial class MessageAssemblerTest
    {
        [AttributeUsage(AttributeTargets.Method)]
        private sealed class ResponseTypeAttribute : Attribute
        {
            public ResponseTypeAttribute(Type type)
            {
                Type = type;
            }

            public Type Type { get; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class HeaderResponseTypeAttribute : Attribute
        {
            public HeaderResponseTypeAttribute(Type type, int[] indexes, int streamIndex)
            {
                Type = type;
                Indexes = indexes;
                StreamIndex = streamIndex;
            }

            public Type Type { get; }

            public int[] Indexes { get; }

            public int StreamIndex { get; }
        }

        [ServiceContract]
        private sealed class ResponseTypeCases
        {
            [OperationContract]
            [ResponseType(typeof(Message))]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message))]
            public Task Task() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message))]
            public ValueTask ValueTask() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            public string String() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<(string, int)>))]
            public (string Value1, int Value2) ValueTuple() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<int?>))]
            public int? NullableInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            public Task<string> TaskString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<int?>))]
            public ValueTask<int?> ValueTaskInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<(string, int)>))]
            public Task<(string Value1, int Value2)> TaskValueTuple() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<int>))]
            public IAsyncEnumerable<int> AsyncEnumerableInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            public Task<IAsyncEnumerable<string>> TaskAsyncEnumerableString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            public ValueTask<IAsyncEnumerable<string>> ValueTaskAsyncEnumerableString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            [HeaderResponseType(typeof(Message<string, int>), new[] { 0, 2 }, 1)]
            public Task<(string Value1, IAsyncEnumerable<string> Stream, int Value2)> TaskAsyncEnumerableWithHeader() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType(typeof(Message<string>))]
            [HeaderResponseType(typeof(Message<int>), new[] { 1 }, 0)]
            public ValueTask<(IAsyncEnumerable<string> Stream, int Value)> ValueTaskAsyncEnumerableWithHeader() => throw new NotSupportedException();
        }

        [ServiceContract]
        private sealed class NotSupportedResponseTypeCases
        {
            [OperationContract]
            public CallContext CallContext() => throw new NotSupportedException();

            [OperationContract]
            public CallOptions CallOptions() => throw new NotSupportedException();

            [OperationContract]
            public ServerCallContext ServerCallContext() => throw new NotSupportedException();

            [OperationContract]
            public Task<CallOptions> TaskCallOptions() => throw new NotSupportedException();

            [OperationContract]
            public Stream Stream() => throw new NotSupportedException();

            [OperationContract]
            public Task<Stream> TaskStream() => throw new NotSupportedException();

            [OperationContract]
            public ValueTuple<IAsyncEnumerable<string>> SyncAsyncEnumerableInTuple() => throw new NotSupportedException();

            [OperationContract]
            public Task<ValueTuple<IAsyncEnumerable<string>>> SyncAsyncEnumerableInTaskTuple() => throw new NotSupportedException();

            [OperationContract]
            public (IAsyncEnumerable<string> Stream, int Value) SyncAsyncEnumerableWithHeader() => throw new NotSupportedException();

            [OperationContract]
            public Task<(IAsyncEnumerable<string> Stream1, IAsyncEnumerable<string> Stream2)> DoubleAsyncEnumerableWithHeader() => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class RequestTypeAttribute : Attribute
        {
            public RequestTypeAttribute(Type type, int[] indexes)
            {
                Type = type;
                Indexes = indexes;
            }

            public Type Type { get; }

            public int[] Indexes { get; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class HeaderRequestTypeAttribute : Attribute
        {
            public HeaderRequestTypeAttribute(Type type, int[] indexes)
            {
                Type = type;
                Indexes = indexes;
            }

            public Type Type { get; }

            public int[] Indexes { get; }
        }

        [ServiceContract]
        private sealed class RequestTypeCases
        {
            [OperationContract]
            [RequestType(typeof(Message), new int[0])]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [RequestType(typeof(Message<int>), new[] { 0 })]
            public void Int(int value) => throw new NotSupportedException();

            [OperationContract]
            [RequestType(typeof(Message<string, int?>), new[] { 0, 1 })]
            public void StringInt(string value1, int? value2) => throw new NotSupportedException();

            [OperationContract]
            [RequestType(typeof(Message<int>), new[] { 0 })]
            public void AsyncEnumerableInt(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [RequestType(typeof(Message<int>), new[] { 2 })]
            [HeaderRequestType(typeof(Message<int, string>), new[] { 0, 1 })]
            public void AsyncEnumerableInt(int value2, string value3, IAsyncEnumerable<int> value1) => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class ContextInputAttribute : Attribute
        {
            public ContextInputAttribute(int[] indexes)
            {
                Indexes = indexes;
            }

            public int[] Indexes { get; }
        }

        [ServiceContract]
        private sealed class ContextInputCases
        {
            [OperationContract]
            [ContextInput(new int[0])]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [ContextInput(new[] { 0 })]
            public void CallOptions(CallOptions value) => throw new NotSupportedException();

            [OperationContract]
            [ContextInput(new[] { 1 })]
            public void CallContext(int value1, CallContext value2) => throw new NotSupportedException();

            [OperationContract]
            [ContextInput(new[] { 0 })]
            public void ServerCallContext(ServerCallContext value) => throw new NotSupportedException();

            [OperationContract]
            [ContextInput(new[] { 0 })]
            public void CancellationToken(CancellationToken value) => throw new NotSupportedException();

            [OperationContract]
            [ContextInput(new[] { 0, 2 })]
            public void CallOptionsCancellationToken(CallOptions value1, int value2, CancellationToken value3) => throw new NotSupportedException();
        }

        [ServiceContract]
        private sealed class NotSupportedParametersCases
        {
            [OperationContract]
            public void Task(Task value) => throw new NotSupportedException();

            [OperationContract]
            public void Stream(Stream value) => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class OperationTypeAttribute : Attribute
        {
            public OperationTypeAttribute(MethodType type)
            {
                Type = type;
            }

            public MethodType Type { get; }
        }

        [ServiceContract]
        private sealed class OperationTypeCases
        {
            [OperationContract]
            [OperationType(MethodType.Unary)]
            public void UnaryVoid() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.Unary)]
            public int UnaryInt() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.Unary)]
            public Task<int> UnaryTaskInt() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.Unary)]
            public Task<int> UnaryTaskIntString(string value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.ClientStreaming)]
            public void ClientStreamingVoid(IAsyncEnumerable<string> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.ServerStreaming)]
            public IAsyncEnumerable<string> ServerStreaming() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.ServerStreaming)]
            public Task<IAsyncEnumerable<string>> ServerStreamingTask() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.ServerStreaming)]
            public Task<(IAsyncEnumerable<string> Stream, string Value)> ServerStreamingWithResponseHeaders() => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.DuplexStreaming)]
            public IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.DuplexStreaming)]
            public Task<IAsyncEnumerable<string>> DuplexStreamingTask(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType(MethodType.DuplexStreaming)]
            public Task<(IAsyncEnumerable<string> Stream, string Value)> DuplexStreamingWithResponseHeaders(IAsyncEnumerable<int> value) => throw new NotSupportedException();
        }

        [ServiceContract]
        private sealed class GenericNotSupportedCases
        {
            [OperationContract]
            public void Method<T>() => throw new NotSupportedException();
        }
    }
}
