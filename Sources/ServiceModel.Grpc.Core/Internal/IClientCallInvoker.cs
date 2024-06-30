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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace ServiceModel.Grpc.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IClientCallInvoker
{
    CallOptionsBuilder CreateOptionsBuilder();

    void UnaryInvoke<TRequest, TResponse>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class;

    TResult? UnaryInvoke<TRequest, TResponse, TResult>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class;

    Task UnaryInvokeAsync<TRequest, TResponse>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class;

    Task<TResult?> UnaryInvokeAsync<TRequest, TResponse, TResult>(CallInvoker callInvoker, IMethod method, CallOptionsBuilder optionsBuilder, TRequest request)
        where TRequest : class
        where TResponse : class;

    Task ClientInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponse>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponse : class;

    Task<TResult?> ClientInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponse, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponse : class;

    IAsyncEnumerable<TResponseValue?> ServerInvoke<TRequest, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request)
        where TRequest : class
        where TResponse : class, IMessage<TResponseValue>
        where TResponseHeader : class;

    Task<IAsyncEnumerable<TResponseValue?>> ServerInvokeAsync<TRequest, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request)
        where TRequest : class
        where TResponse : class, IMessage<TResponseValue>
        where TResponseHeader : class;

    Task<TResult> ServerInvokeAsync<TRequest, TResponseHeader, TResponse, TResponseValue, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequest request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
        where TRequest : class
        where TResponse : class, IMessage<TResponseValue>
        where TResponseHeader : class;

    IAsyncEnumerable<TResponseValue?> DuplexInvoke<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>;

    Task<IAsyncEnumerable<TResponseValue?>> DuplexInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>;

    Task<TResult> DuplexInvokeAsync<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue, TResult>(
        CallInvoker callInvoker,
        IMethod method,
        CallOptionsBuilder optionsBuilder,
        TRequestHeader? requestHeader,
        IAsyncEnumerable<TRequestValue?> request,
        Func<TResponseHeader, IAsyncEnumerable<TResponseValue?>, TResult> continuationFunction)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>, new()
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>;
}