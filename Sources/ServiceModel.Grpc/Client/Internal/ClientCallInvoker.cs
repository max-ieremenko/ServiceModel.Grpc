// <copyright>
// Copyright Max Ieremenko
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

using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Client.Internal;

internal sealed class ClientCallInvoker : IClientCallInvoker
{
    private readonly Func<CallOptions>? _callOptionsFactory;
    private readonly IClientCallFilterHandlerFactory? _filterHandlerFactory;

    public ClientCallInvoker(Func<CallOptions>? callOptionsFactory, IClientCallFilterHandlerFactory? filterHandlerFactory)
    {
        _callOptionsFactory = callOptionsFactory;
        _filterHandlerFactory = filterHandlerFactory;
    }

    public CallOptionsBuilder CreateOptionsBuilder() => new(_callOptionsFactory);

    public void UnaryInvoke<TRequest, TResponse>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        new UnaryCall<TRequest, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory).Invoke(request);
    }

    public TResult? UnaryInvoke<TRequest, TResponse, TResult>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        return new UnaryCall<TRequest, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory).Invoke<TResult>(request);
    }

    public Task UnaryInvokeAsync<TRequest, TResponse>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        return new UnaryCall<TRequest, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory).InvokeAsync(request);
    }

    public Task<TResult?> UnaryInvokeAsync<TRequest, TResponse, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request)
        where TRequest : class
        where TResponse : class
    {
        return new UnaryCall<TRequest, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory).InvokeAsync<TResult>(request);
    }

    public Task ClientInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponse>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponse : class
    {
        return new ClientStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory, requestHeader)
            .InvokeAsync(request);
    }

    public Task<TResult?> ClientInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponse, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponse : class
    {
        return new ClientStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponse>(method, callInvoker, optionsBuilder, _filterHandlerFactory, requestHeader)
            .InvokeAsync<TResult>(request);
    }

    public IAsyncEnumerable<TResponseValue?> ServerInvoke<TRequest, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new ServerStreamingCall<TRequest, TResponseHeader, TResponse, TResponseValue>(method, callInvoker, optionsBuilder, _filterHandlerFactory)
            .Invoke(request);
    }

    public Task<IAsyncEnumerable<TResponseValue?>> ServerInvokeAsync<TRequest, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new ServerStreamingCall<TRequest, TResponseHeader, TResponse, TResponseValue>(method, callInvoker, optionsBuilder, _filterHandlerFactory)
            .InvokeAsync(request);
    }

    public Task<TResult> ServerInvokeAsync<TRequest, TResponseHeader, TResponse, TResponseValue, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new ServerStreamingCall<TRequest, TResponseHeader, TResponse, TResponseValue>(method, callInvoker, optionsBuilder, _filterHandlerFactory)
            .InvokeAsync(request, continuationFunction);
    }

    public IAsyncEnumerable<TResponseValue?> DuplexInvoke<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new DuplexStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
                method,
                callInvoker,
                optionsBuilder,
                _filterHandlerFactory,
                requestHeader)
            .Invoke(request);
    }

    public Task<IAsyncEnumerable<TResponseValue?>> DuplexInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new DuplexStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
                method,
                callInvoker,
                optionsBuilder,
                _filterHandlerFactory,
                requestHeader)
            .InvokeAsync(request);
    }

    public Task<TResult> DuplexInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>
    {
        return new DuplexStreamingCall<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
                method,
                callInvoker,
                optionsBuilder,
                _filterHandlerFactory,
                requestHeader)
            .InvokeAsync(request, continuationFunction);
    }
}