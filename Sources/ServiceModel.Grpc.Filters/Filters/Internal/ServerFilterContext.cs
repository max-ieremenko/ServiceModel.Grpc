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

using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal sealed class ServerFilterContext : IServerFilterContextInternal
{
    private readonly IOperationDescriptor _operation;
    private MethodInfo? _serviceMethodInfo;

    public ServerFilterContext(
        object serviceInstance,
        ServerCallContext serverCallContext,
        IServiceProvider serviceProvider,
        IOperationDescriptor operation,
        IRequestContextInternal request,
        IResponseContextInternal response)
    {
        _operation = operation;
        ServiceInstance = serviceInstance;
        ServerCallContext = serverCallContext;
        ServiceProvider = serviceProvider;
        RequestInternal = request;
        ResponseInternal = response;
    }

    public object ServiceInstance { get; }

    public ServerCallContext ServerCallContext { get; }

    public IServiceProvider ServiceProvider { get; }

    public IDictionary<object, object> UserState => ServerCallContext.UserState;

    public MethodInfo ContractMethodInfo => _operation.GetContractMethod();

    public MethodInfo ServiceMethodInfo
    {
        get
        {
            if (_serviceMethodInfo == null)
            {
                _serviceMethodInfo = ReflectionTools.ImplementationOfMethod(ServiceInstance.GetType(), ContractMethodInfo);
            }

            return _serviceMethodInfo;
        }
    }

    IRequestContext IServerFilterContext.Request => RequestInternal;

    IResponseContext IServerFilterContext.Response => ResponseInternal;

    public IRequestContextInternal RequestInternal { get; }

    public IResponseContextInternal ResponseInternal { get; }

    public override string ToString() => $"{ServerCallContext.Method} - {ContractMethodInfo.DeclaringType}.{ContractMethodInfo.Name}()";
}