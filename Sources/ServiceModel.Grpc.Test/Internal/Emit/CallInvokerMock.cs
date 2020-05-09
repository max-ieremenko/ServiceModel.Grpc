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
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using ServiceModel.Grpc.Channel;
using Shouldly;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class CallInvokerMock
    {
        public static void SetupBlockingUnaryCall(this Mock<CallInvoker> invoker, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.BlockingUnaryCall(
                    It.IsNotNull<Method<Message, Message>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message>()))
                .Callback<Method<Message, Message>, string, CallOptions, Message>((method, _, options, request) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                })
                .Returns(new Message());
        }

        public static void SetupBlockingUnaryCallOut<TResponse>(this Mock<CallInvoker> invoker, TResponse response, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.BlockingUnaryCall(
                    It.IsNotNull<Method<Message, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message>()))
                .Callback<Method<Message, Message<TResponse>>, string, CallOptions, Message>((method, _, options, request) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                })
                .Returns(new Message<TResponse>(response));
        }

        public static void SetupBlockingUnaryCallIn<TRequest>(this Mock<CallInvoker> invoker, TRequest request, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.BlockingUnaryCall(
                    It.IsNotNull<Method<Message<TRequest>, Message>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest>>()))
                .Callback<Method<Message<TRequest>, Message>, string, CallOptions, Message<TRequest>>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                    r.Value1.ShouldBe(request);
                })
                .Returns(new Message());
        }

        public static void SetupBlockingUnaryCallInOut<TRequest, TResponse>(
            this Mock<CallInvoker> invoker,
            TRequest request,
            TResponse response,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.BlockingUnaryCall(
                    It.IsNotNull<Method<Message<TRequest>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest>>()))
                .Callback<Method<Message<TRequest>, Message<TResponse>>, string, CallOptions, Message<TRequest>>((method, _, options, message) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                    message.Value1.ShouldBe(request);
                })
                .Returns(new Message<TResponse>(response));
        }

        public static void SetupBlockingUnaryCallInOut<TRequest1, TRequest2, TResponse>(
            this Mock<CallInvoker> invoker,
            TRequest1 request1,
            TRequest2 request2,
            TResponse response,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.BlockingUnaryCall(
                    It.IsNotNull<Method<Message<TRequest1, TRequest2>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest1, TRequest2>>()))
                .Callback<Method<Message<TRequest1, TRequest2>, Message<TResponse>>, string, CallOptions, Message<TRequest1, TRequest2>>((method, _, options, request) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                    request.Value1.ShouldBe(request1);
                    request.Value2.ShouldBe(request2);
                })
                .Returns(new Message<TResponse>(response));
        }

        public static void SetupAsyncUnaryCall(this Mock<CallInvoker> invoker, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncUnaryCall(
                    It.IsNotNull<Method<Message, Message>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message>()))
                .Callback<Method<Message, Message>, string, CallOptions, Message>((method, _, options, request) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncUnaryCall<Message>(
                    Task.FromResult(new Message()),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncUnaryCallOut<TResponse>(this Mock<CallInvoker> invoker, TResponse response, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncUnaryCall(
                    It.IsNotNull<Method<Message, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message>()))
                .Callback<Method<Message, Message<TResponse>>, string, CallOptions, Message>((method, _, options, request) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncUnaryCall<Message<TResponse>>(
                    Task.FromResult(new Message<TResponse>(response)),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncUnaryCallIn<TRequest>(this Mock<CallInvoker> invoker, TRequest request, Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncUnaryCall(
                    It.IsNotNull<Method<Message<TRequest>, Message>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest>>()))
                .Callback<Method<Message<TRequest>, Message>, string, CallOptions, Message<TRequest>>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                    r.Value1.ShouldBe(request);
                })
                .Returns(new AsyncUnaryCall<Message>(
                    Task.FromResult(new Message()),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncUnaryCallInOut<TRequest1, TRequest2, TRequest3, TResponse>(
            this Mock<CallInvoker> invoker,
            TRequest1 request1,
            TRequest2 request2,
            TRequest3 request3,
            TResponse response,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncUnaryCall(
                    It.IsNotNull<Method<Message<TRequest1, TRequest2, TRequest3>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest1, TRequest2, TRequest3>>()))
                .Callback<Method<Message<TRequest1, TRequest2, TRequest3>, Message<TResponse>>, string, CallOptions, Message<TRequest1, TRequest2, TRequest3>>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.Unary);
                    callOptions?.Invoke(options);
                    r.Value1.ShouldBe(request1);
                    r.Value2.ShouldBe(request2);
                    r.Value3.ShouldBe(request3);
                })
                .Returns(new AsyncUnaryCall<Message<TResponse>>(
                    Task.FromResult(new Message<TResponse>(response)),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncServerStreamingCall<TResponse>(
            this Mock<CallInvoker> invoker,
            IAsyncStreamReader<Message<TResponse>> responseStream,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncServerStreamingCall(
                    It.IsNotNull<Method<Message, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message>()))
                .Callback<Method<Message, Message<TResponse>>, string, CallOptions, Message>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.ServerStreaming);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncServerStreamingCall<Message<TResponse>>(
                    responseStream,
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncServerStreamingCall<TRequest, TResponse>(
            this Mock<CallInvoker> invoker,
            TRequest request,
            IAsyncStreamReader<Message<TResponse>> responseStream,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncServerStreamingCall(
                    It.IsNotNull<Method<Message<TRequest>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest>>()))
                .Callback<Method<Message<TRequest>, Message<TResponse>>, string, CallOptions, Message<TRequest>>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.ServerStreaming);
                    callOptions?.Invoke(options);
                    r.Value1.ShouldBe(request);
                })
                .Returns(new AsyncServerStreamingCall<Message<TResponse>>(
                    responseStream,
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncServerStreamingCall<TRequest1, TRequest2, TResponse>(
            this Mock<CallInvoker> invoker,
            TRequest1 request1,
            TRequest2 request2,
            IAsyncStreamReader<Message<TResponse>> responseStream,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncServerStreamingCall(
                    It.IsNotNull<Method<Message<TRequest1, TRequest2>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>(),
                    It.IsNotNull<Message<TRequest1, TRequest2>>()))
                .Callback<Method<Message<TRequest1, TRequest2>, Message<TResponse>>, string, CallOptions, Message<TRequest1, TRequest2>>((method, _, options, r) =>
                {
                    method.Type.ShouldBe(MethodType.ServerStreaming);
                    callOptions?.Invoke(options);
                    r.Value1.ShouldBe(request1);
                    r.Value2.ShouldBe(request2);
                })
                .Returns(new AsyncServerStreamingCall<Message<TResponse>>(
                    responseStream,
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncClientStreamingCall<TRequest>(
            this Mock<CallInvoker> invoker,
            IClientStreamWriter<Message<TRequest>> requestStream,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncClientStreamingCall(
                    It.IsNotNull<Method<Message<TRequest>, Message>>(),
                    null,
                    It.IsAny<CallOptions>()))
                .Callback<Method<Message<TRequest>, Message>, string, CallOptions>((method, _, options) =>
                {
                    method.Type.ShouldBe(MethodType.ClientStreaming);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncClientStreamingCall<Message<TRequest>, Message>(
                    requestStream,
                    Task.FromResult(new Message()),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncClientStreamingCall<TRequest, TResponse>(
            this Mock<CallInvoker> invoker,
            IClientStreamWriter<Message<TRequest>> requestStream,
            TResponse response,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncClientStreamingCall(
                    It.IsNotNull<Method<Message<TRequest>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>()))
                .Callback<Method<Message<TRequest>, Message<TResponse>>, string, CallOptions>((method, _, options) =>
                {
                    method.Type.ShouldBe(MethodType.ClientStreaming);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncClientStreamingCall<Message<TRequest>, Message<TResponse>>(
                    requestStream,
                    Task.FromResult(new Message<TResponse>(response)),
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }

        public static void SetupAsyncDuplexStreamingCall<TRequest, TResponse>(
            this Mock<CallInvoker> invoker,
            IClientStreamWriter<Message<TRequest>> requestStream,
            IAsyncStreamReader<Message<TResponse>> responseStream,
            Action<CallOptions> callOptions = null)
        {
            invoker
                .Setup(i => i.AsyncDuplexStreamingCall(
                    It.IsNotNull<Method<Message<TRequest>, Message<TResponse>>>(),
                    null,
                    It.IsAny<CallOptions>()))
                .Callback<Method<Message<TRequest>, Message<TResponse>>, string, CallOptions>((method, _, options) =>
                {
                    method.Type.ShouldBe(MethodType.DuplexStreaming);
                    callOptions?.Invoke(options);
                })
                .Returns(new AsyncDuplexStreamingCall<Message<TRequest>, Message<TResponse>>(
                    requestStream,
                    responseStream,
                    _ => Task.FromResult(default(Metadata)),
                    _ => default,
                    _ => default,
                    _ =>
                    {
                    },
                    null));
        }
    }
}
