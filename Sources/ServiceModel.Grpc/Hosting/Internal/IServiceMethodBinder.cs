﻿// <copyright>
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
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;

#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented

namespace ServiceModel.Grpc.Hosting.Internal;

/// <summary>
/// This API supports ServiceModel.Grpc infrastructure and is not intended to be used directly from your code.
/// This API may change or be removed in future releases.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IServiceMethodBinder<TService>
{
    IMarshallerFactory MarshallerFactory { get; }

    void AddUnaryMethod<TRequest, TResponse>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
        where TRequest : class
        where TResponse : class;

    void AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, Task<TResponse>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponse : class;

    void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new();

    void AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<MethodInfo> resolveContractMethodDefinition,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new();
}