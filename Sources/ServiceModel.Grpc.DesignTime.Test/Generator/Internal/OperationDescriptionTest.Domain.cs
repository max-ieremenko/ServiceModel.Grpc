// <copyright>
// Copyright 2020-2022 Max Ieremenko
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

namespace ServiceModel.Grpc.DesignTime.Generator.Internal
{
    public partial class OperationDescriptionTest
    {
        [AttributeUsage(AttributeTargets.Method)]
        private sealed class ResponseTypeAttribute : Attribute
        {
            public ResponseTypeAttribute(string className, string? valueTypeName)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class HeaderResponseTypeAttribute : Attribute
        {
            public HeaderResponseTypeAttribute(string className, int[] indexes, string[] valueTypeName, int streamIndex)
            {
            }
        }

        [ServiceContract]
        private sealed class ResponseTypeCases
        {
            [OperationContract]
            [ResponseType("Message", null)]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message", null)]
            public Task Task() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message", null)]
            public ValueTask ValueTask() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            public string String() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<ValueTuple<string, int>>", "ValueTuple<string, int>")]
            public (string Value1, int Value2) ValueTuple() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<int?>", "int?")]
            public int? NullableInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            public Task<string> TaskString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<int?>", "int?")]
            public ValueTask<int?> ValueTaskInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<ValueTuple<string, int>>", "ValueTuple<string, int>")]
            public Task<(string Value1, int Value2)> TaskValueTuple() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<int>", "int")]
            public IAsyncEnumerable<int> AsyncEnumerableInt() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            public Task<IAsyncEnumerable<string>> TaskAsyncEnumerableString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            public ValueTask<IAsyncEnumerable<string>> ValueTaskAsyncEnumerableString() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            [HeaderResponseType("Message<string, int>", new[] { 0, 2 }, new[] { "string", "int" }, 1)]
            public Task<(string Value1, IAsyncEnumerable<string> Stream, int Value2)> TaskAsyncEnumerableWithHeader() => throw new NotSupportedException();

            [OperationContract]
            [ResponseType("Message<string>", "string")]
            [HeaderResponseType("Message<int>", new[] { 1 }, new[] { "int" }, 0)]
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
            public CallOptions? NullableCallOptions() => throw new NotSupportedException();

            [OperationContract]
            public CancellationToken CancellationToken() => throw new NotSupportedException();

            [OperationContract]
            public CancellationToken? NullableCancellationToken() => throw new NotSupportedException();

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
            public RequestTypeAttribute(string className, int[] indexes, string[] valueTypeName)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class HeaderRequestTypeAttribute : Attribute
        {
            public HeaderRequestTypeAttribute(string className, int[] indexes, string[] valueTypeName)
            {
            }
        }

        [ServiceContract]
        private sealed class RequestTypeCases
        {
            [OperationContract]
            [RequestType("Message", new int[0], new string[0])]
            public void Void() => throw new NotSupportedException();

            [OperationContract]
            [RequestType("Message<int>", new[] { 0 }, new[] { "int" })]
            public void Int(int value) => throw new NotSupportedException();

            [OperationContract]
            [RequestType("Message<string, int?>", new[] { 0, 1 }, new[] { "string", "int?" })]
            public void StringInt(string value1, int? value2) => throw new NotSupportedException();

            [OperationContract]
            [RequestType("Message<int>", new[] { 0 }, new[] { "int" })]
            public void AsyncEnumerableInt(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [RequestType("Message<int>", new[] { 2 }, new[] { "int" })]
            [HeaderRequestType("Message<int, string>", new[] { 0, 1 }, new[] { "int", "string" })]
            public void AsyncEnumerableInt(int value2, string value3, IAsyncEnumerable<int> value1) => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private sealed class ContextInputAttribute : Attribute
        {
            public ContextInputAttribute(int[] indexes)
            {
            }
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
            [ContextInput(new[] { 0 })]
            public void NullableCallOptions(CallOptions? value) => throw new NotSupportedException();

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
            [ContextInput(new[] { 0 })]
            public void NullableCancellationToken(CancellationToken? value) => throw new NotSupportedException();

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
            public OperationTypeAttribute(string type)
            {
            }
        }

        [ServiceContract]
        private sealed class OperationTypeCases
        {
            [OperationContract]
            [OperationType("Unary")]
            public void UnaryVoid() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("Unary")]
            public int UnaryInt() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("Unary")]
            public Task<int> UnaryTaskInt() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("Unary")]
            public Task<int> UnaryTaskIntString(string value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType("ClientStreaming")]
            public void ClientStreamingVoid(IAsyncEnumerable<string> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType("ServerStreaming")]
            public IAsyncEnumerable<string> ServerStreaming() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("ServerStreaming")]
            public Task<IAsyncEnumerable<string>> ServerStreamingTask() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("ServerStreaming")]
            public Task<(IAsyncEnumerable<string> Stream, string Value)> ServerStreamingWithResponseHeaders() => throw new NotSupportedException();

            [OperationContract]
            [OperationType("DuplexStreaming")]
            public IAsyncEnumerable<string> DuplexStreaming(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType("DuplexStreaming")]
            public Task<IAsyncEnumerable<string>> DuplexStreamingTask(IAsyncEnumerable<int> value) => throw new NotSupportedException();

            [OperationContract]
            [OperationType("DuplexStreaming")]
            public Task<(IAsyncEnumerable<string> Stream, string Value)> DuplexStreamingWithResponseHeaders(IAsyncEnumerable<int> value) => throw new NotSupportedException();
        }

        [ServiceContract]
        private sealed class GenericNotSupportedCases
        {
            [OperationContract]
            public void Method<T>() => throw new NotSupportedException();
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private sealed class CompatibleToAttribute : Attribute
        {
            public CompatibleToAttribute(string methodName)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private sealed class NotCompatibleToAttribute : Attribute
        {
            public NotCompatibleToAttribute(string methodName)
            {
            }
        }

        private sealed class IsCompatibleToCases
        {
            [CompatibleTo(nameof(Empty1))]
            [CompatibleTo(nameof(Empty2))]
            [CompatibleTo(nameof(EmptyAsync1))]
            [CompatibleTo(nameof(EmptyAsync2))]
            [NotCompatibleTo(nameof(Empty3))]
            [NotCompatibleTo(nameof(Empty4))]
            public void Empty() => throw new NotSupportedException();

            public void Empty1() => throw new NotSupportedException();

            public void Empty2(CancellationToken token, CallContext context) => throw new NotSupportedException();

            public string Empty3() => throw new NotSupportedException();

            public void Empty4(string x) => throw new NotSupportedException();

            public Task EmptyAsync1() => throw new NotSupportedException();

            public ValueTask EmptyAsync2(CallContext context) => throw new NotSupportedException();

            [CompatibleTo(nameof(Data1))]
            [CompatibleTo(nameof(Data2))]
            [NotCompatibleTo(nameof(Data3))]
            [NotCompatibleTo(nameof(Data4))]
            public string Data(int x, string y) => throw new NotSupportedException();

            public Task<string?> Data1(int x, string y) => throw new NotSupportedException();

            public ValueTask<string> Data2(int x, string? y) => throw new NotSupportedException();

            public Task Data3(int x, string y) => throw new NotSupportedException();

            public string Data4(int? x, string y) => throw new NotSupportedException();
        }
    }
}
