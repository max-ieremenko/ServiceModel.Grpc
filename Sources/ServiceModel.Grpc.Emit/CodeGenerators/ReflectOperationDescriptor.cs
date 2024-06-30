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
using Grpc.Core;
using ServiceModel.Grpc.Descriptions;
using ServiceModel.Grpc.Emit.Descriptions;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

internal sealed class ReflectOperationDescriptor : IOperationDescriptor
{
    public static readonly string[] UnaryResultNames = { "result" };

    private readonly Func<MethodInfo> _getContractMethod;
    private MethodInfo? _contractMethod;
    private IMessageAccessor? _request;
    private IStreamAccessor? _requestStream;
    private IMessageAccessor? _response;
    private IStreamAccessor? _responseStream;
    private int[]? _requestHeaderParameters;
    private int[]? _requestParameters;

    public ReflectOperationDescriptor(Func<MethodInfo> getContractMethod)
    {
        _getContractMethod = getContractMethod;
    }

    public MethodInfo GetContractMethod()
    {
        if (_contractMethod == null)
        {
            _contractMethod = _getContractMethod();
        }

        return _contractMethod;
    }

    public bool IsAsync() => ReflectionTools.IsTask(GetContractMethod().ReturnType);

    public int[] GetRequestHeaderParameters()
    {
        Init();
        return _requestHeaderParameters!;
    }

    public int[] GetRequestParameters()
    {
        Init();
        return _requestParameters!;
    }

    public IMessageAccessor GetRequestAccessor()
    {
        Init();
        return _request!;
    }

    public IStreamAccessor? GetRequestStreamAccessor()
    {
        Init();
        return _requestStream;
    }

    public IMessageAccessor GetResponseAccessor()
    {
        Init();
        return _response!;
    }

    public IStreamAccessor? GetResponseStreamAccessor()
    {
        Init();
        return _responseStream;
    }

    private static (Type MessageType, string[] Names) GetRequest(OperationDescription<Type> operation)
    {
        Type requestType;
        int[] requestTypeInput;
        if (operation.OperationType == MethodType.Unary || operation.OperationType == MethodType.ServerStreaming)
        {
            requestType = operation.RequestType.GetClrType();
            requestTypeInput = operation.RequestTypeInput;
        }
        else
        {
            requestType = operation.HeaderRequestType.GetClrType();
            requestTypeInput = operation.HeaderRequestTypeInput;
        }

        if (requestTypeInput.Length == 0)
        {
            return (requestType, []);
        }

        var requestNames = new string[requestTypeInput.Length];
        for (var i = 0; i < requestNames.Length; i++)
        {
            var index = requestTypeInput[i];
            requestNames[i] = operation.Method.Parameters[index].Name;
        }

        return (requestType, requestNames);
    }

    private static (Type MessageType, string[] Names) GetResponse(OperationDescription<Type> operation)
    {
        Type responseType;
        string[] names;
        if (operation.OperationType == MethodType.Unary || operation.OperationType == MethodType.ClientStreaming)
        {
            responseType = operation.ResponseType.GetClrType();
            names = operation.ResponseType.IsGenericType() ? UnaryResultNames : [];
        }
        else
        {
            responseType = operation.HeaderResponseType.GetClrType();
            names = operation.GetResponseHeaderNames();
        }

        return (responseType, names);
    }

    private static Type? GetRequestStream(OperationDescription<Type> operation)
    {
        if (operation.OperationType != MethodType.ClientStreaming && operation.OperationType != MethodType.DuplexStreaming)
        {
            return null;
        }

        return operation.RequestType.Properties[0];
    }

    private static Type? GetResponseStream(OperationDescription<Type> operation)
    {
        if (operation.OperationType != MethodType.ServerStreaming && operation.OperationType != MethodType.DuplexStreaming)
        {
            return null;
        }

        return operation.ResponseType.Properties[0];
    }

    private void Init()
    {
        if (_request != null)
        {
            return;
        }

        var operation = ContractDescriptionBuilder.BuildOperation(GetContractMethod(), "dummy", "dummy");

        var (messageType, messageNames) = GetRequest(operation);
        _request = new ReflectMessageAccessor(messageType, messageNames);

        var streamType = GetRequestStream(operation);
        _requestStream = streamType == null ? null : new ReflectStreamAccessor(streamType);

        (messageType, messageNames) = GetResponse(operation);
        _response = new ReflectMessageAccessor(messageType, messageNames);

        streamType = GetResponseStream(operation);
        _responseStream = streamType == null ? null : new ReflectStreamAccessor(streamType);

        _requestHeaderParameters = operation.HeaderRequestTypeInput;
        _requestParameters = operation.RequestTypeInput;
    }
}