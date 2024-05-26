// <copyright>
// Copyright 2021 Max Ieremenko
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

internal sealed class ServerFilterContext : IServerFilterContextInternal
{
    private readonly Func<object, MethodInfo> _getServiceMethodInfo;

    public ServerFilterContext(
        object serviceInstance,
        ServerCallContext serverCallContext,
        IServiceProvider serviceProvider,
        MethodInfo contractMethodInfo,
        Func<object, MethodInfo> getServiceMethodInfo,
        IRequestContextInternal request,
        IResponseContextInternal response)
    {
        ServiceInstance = serviceInstance;
        ServerCallContext = serverCallContext;
        ServiceProvider = serviceProvider;
        ContractMethodInfo = contractMethodInfo;
        _getServiceMethodInfo = getServiceMethodInfo;
        RequestInternal = request;
        ResponseInternal = response;
    }

    public object ServiceInstance { get; }

    public ServerCallContext ServerCallContext { get; }

    public IServiceProvider ServiceProvider { get; }

    public IDictionary<object, object> UserState => ServerCallContext.UserState;

    public MethodInfo ContractMethodInfo { get; }

    public MethodInfo ServiceMethodInfo => _getServiceMethodInfo(ServiceInstance);

    IRequestContext IServerFilterContext.Request => RequestInternal;

    IResponseContext IServerFilterContext.Response => ResponseInternal;

    public IRequestContextInternal RequestInternal { get; }

    public IResponseContextInternal ResponseInternal { get; }

    public override string ToString() => $"{ServerCallContext.Method} - {ContractMethodInfo.DeclaringType}.{ContractMethodInfo.Name}()";
}