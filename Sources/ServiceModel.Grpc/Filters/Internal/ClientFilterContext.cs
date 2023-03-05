// <copyright>
// Copyright 2023 Max Ieremenko
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
using Grpc.Core;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ClientFilterContext : IClientFilterContextInternal
{
    private readonly Func<MethodInfo> _resolveContractMethodInfo;

    private MethodInfo? _contractMethodInfo;
    private IDictionary<object, object>? _userState;

    public ClientFilterContext(
        IServiceProvider? serviceProvider,
        CallInvoker callInvoker,
        CallOptions callOptions,
        IMethod method,
        Func<MethodInfo> resolveContractMethodInfo,
        IRequestContextInternal request,
        IResponseContextInternal response)
    {
        _resolveContractMethodInfo = resolveContractMethodInfo;
        ServiceProvider = serviceProvider!;
        CallInvoker = callInvoker;
        CallOptions = callOptions;
        Method = method;
        RequestInternal = request;
        ResponseInternal = response;
    }

    public CallOptions CallOptions { get; }

    public IMethod Method { get; }

    public IServiceProvider ServiceProvider { get; }

    public IDictionary<object, object> UserState
    {
        get
        {
            if (_userState == null)
            {
                _userState = new Dictionary<object, object>();
            }

            return _userState;
        }
    }

    public CallInvoker CallInvoker { get; }

    public CallContext? CallContext { get; set; }

    public object? RequestHeaderMarshaller { get; set; }

    public object? ResponseHeaderMarshaller { get; set; }

    public MethodInfo ContractMethodInfo
    {
        get
        {
            if (_contractMethodInfo == null)
            {
                _contractMethodInfo = _resolveContractMethodInfo();
            }

            return _contractMethodInfo;
        }
    }

    public IRequestContextInternal RequestInternal { get; }

    public IResponseContextInternal ResponseInternal { get; }

    IRequestContext IClientFilterContext.Request => RequestInternal;

    IResponseContext IClientFilterContext.Response => ResponseInternal;
}