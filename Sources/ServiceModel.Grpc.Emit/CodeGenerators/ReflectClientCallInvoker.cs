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
using System.Reflection;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class ReflectClientCallInvoker
{
    private readonly MethodInfo[] _methods = typeof(IClientCallInvoker).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    private MethodInfo? _createOptionsBuilder;

    private MethodInfo? _unaryInvokeVoid;
    private MethodInfo? _unaryInvokeResult;
    private MethodInfo? _unaryInvokeAsyncVoid;
    private MethodInfo? _unaryInvokeAsyncResult;

    private MethodInfo? _serverInvoke;
    private MethodInfo? _serverInvokeAsync;
    private MethodInfo? _serverInvokeAsyncHeader;

    private MethodInfo? _clientInvokeAsyncVoid;
    private MethodInfo? _clientInvokeAsyncResult;

    private MethodInfo? _duplexInvoke;
    private MethodInfo? _duplexInvokeAsync;
    private MethodInfo? _duplexInvokeAsyncHeader;

    public MethodInfo CreateOptionsBuilder() => FindMethod(ref _createOptionsBuilder, nameof(IClientCallInvoker.CreateOptionsBuilder), 0);

    public MethodInfo Unary(MessageDescription<Type> requestType, MessageDescription<Type> responseType, bool isAsync)
    {
        if (responseType.IsGenericType())
        {
            var result = isAsync
                ? FindMethod(ref _unaryInvokeAsyncResult, nameof(IClientCallInvoker.UnaryInvokeAsync), 3)
                : FindMethod(ref _unaryInvokeResult, nameof(IClientCallInvoker.UnaryInvoke), 3);

            return result.MakeGenericMethod(requestType.GetClrType(), responseType.GetClrType(), responseType.Properties[0]);
        }
        else
        {
            var result = isAsync
                ? FindMethod(ref _unaryInvokeAsyncVoid, nameof(IClientCallInvoker.UnaryInvokeAsync), 2)
                : FindMethod(ref _unaryInvokeVoid, nameof(IClientCallInvoker.UnaryInvoke), 2);

            return result.MakeGenericMethod(requestType.GetClrType(), responseType.GetClrType());
        }
    }

    public MethodInfo Server(
        MessageDescription<Type> requestType,
        MessageDescription<Type>? headerResponseType,
        MessageDescription<Type> responseType,
        Type returnType,
        bool isAsync)
    {
        if (headerResponseType == null && isAsync)
        {
            var result = FindMethod(ref _serverInvokeAsync, nameof(IClientCallInvoker.ServerInvokeAsync), 4);
            return result.MakeGenericMethod(requestType.GetClrType(), headerResponseType.GetClrType(), responseType.GetClrType(), responseType.Properties[0]);
        }

        if (headerResponseType == null)
        {
            var result = FindMethod(ref _serverInvoke, nameof(IClientCallInvoker.ServerInvoke), 4);
            return result.MakeGenericMethod(requestType.GetClrType(), headerResponseType.GetClrType(), responseType.GetClrType(), responseType.Properties[0]);
        }
        else
        {
            var result = FindMethod(ref _serverInvokeAsyncHeader, nameof(IClientCallInvoker.ServerInvokeAsync), 5);
            return result.MakeGenericMethod(requestType.GetClrType(), headerResponseType.GetClrType(), responseType.GetClrType(), responseType.Properties[0], returnType.GenericTypeArguments[0]);
        }
    }

    public MethodInfo Client(MessageDescription<Type>? headerRequestType, MessageDescription<Type> requestType, MessageDescription<Type> responseType)
    {
        if (responseType.IsGenericType())
        {
            var result = FindMethod(ref _clientInvokeAsyncResult, nameof(IClientCallInvoker.ClientInvokeAsync), 5);
            return result.MakeGenericMethod(headerRequestType.GetClrType(), requestType.GetClrType(), requestType.Properties[0], responseType.GetClrType(), responseType.Properties[0]);
        }
        else
        {
            var result = FindMethod(ref _clientInvokeAsyncVoid, nameof(IClientCallInvoker.ClientInvokeAsync), 4);
            return result.MakeGenericMethod(headerRequestType.GetClrType(), requestType.GetClrType(), requestType.Properties[0], responseType.GetClrType());
        }
    }

    public MethodInfo Duplex(
        MessageDescription<Type>? headerRequestType,
        MessageDescription<Type> requestType,
        MessageDescription<Type>? headerResponseType,
        MessageDescription<Type> responseType,
        Type returnType,
        bool isAsync)
    {
        if (headerResponseType == null && isAsync)
        {
            var result = FindMethod(ref _duplexInvokeAsync, nameof(IClientCallInvoker.DuplexInvokeAsync), 6);
            return result.MakeGenericMethod(
                headerRequestType.GetClrType(),
                requestType.GetClrType(),
                requestType.Properties[0],
                headerResponseType.GetClrType(),
                responseType.GetClrType(),
                responseType.Properties[0]);
        }

        if (headerResponseType == null)
        {
            var result = FindMethod(ref _duplexInvoke, nameof(IClientCallInvoker.DuplexInvoke), 6);
            return result.MakeGenericMethod(
                headerRequestType.GetClrType(),
                requestType.GetClrType(),
                requestType.Properties[0],
                headerResponseType.GetClrType(),
                responseType.GetClrType(),
                responseType.Properties[0]);
        }
        else
        {
            var result = FindMethod(ref _duplexInvokeAsyncHeader, nameof(IClientCallInvoker.DuplexInvokeAsync), 7);
            return result.MakeGenericMethod(
                headerRequestType.GetClrType(),
                requestType.GetClrType(),
                requestType.Properties[0],
                headerResponseType.GetClrType(),
                responseType.GetClrType(),
                responseType.Properties[0],
                returnType.GenericTypeArguments[0]);
        }
    }

    private MethodInfo FindMethod(ref MethodInfo? result, string name, int genericArgsCount)
    {
        if (result == null)
        {
            for (var i = 0; i < _methods.Length; i++)
            {
                var method = _methods[i];
                if (name.Equals(method.Name, StringComparison.Ordinal) && method.GetGenericArguments().Length == genericArgsCount)
                {
                    result = method;
                    break;
                }
            }
        }

        return result ?? throw new InvalidOperationException($"Method {nameof(IClientCallInvoker)}.{name}<{genericArgsCount}> not found.");
    }
}