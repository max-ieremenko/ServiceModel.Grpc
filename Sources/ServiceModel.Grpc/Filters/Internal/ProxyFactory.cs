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
using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal;

internal readonly struct ProxyFactory
{
    public ProxyFactory(MethodInfo contractMethodDefinition)
    {
        var operation = new MessageAssembler(contractMethodDefinition);
        RequestProxy = CreateRequestProxy(operation);
        ResponseProxy = CreateResponseProxy(operation);
        RequestStreamProxy = CreateRequestStreamProxy(operation);
        ResponseStreamProxy = CreateResponseStreamProxy(operation);
    }

    public MessageProxy RequestProxy { get; }

    public MessageProxy ResponseProxy { get; }

    public StreamProxy? RequestStreamProxy { get; }

    public StreamProxy? ResponseStreamProxy { get; }

    private static MessageProxy CreateRequestProxy(MessageAssembler operation)
    {
        Type requestType;
        int[] requestTypeInput;
        if (operation.OperationType == MethodType.Unary || operation.OperationType == MethodType.ServerStreaming)
        {
            requestType = operation.RequestType;
            requestTypeInput = operation.RequestTypeInput;
        }
        else
        {
            requestType = operation.HeaderRequestType ?? typeof(Message);
            requestTypeInput = operation.HeaderRequestTypeInput;
        }

        if (requestTypeInput.Length == 0)
        {
            return new MessageProxy(Array.Empty<string>(), requestType);
        }

        var requestNames = new string[requestTypeInput.Length];
        for (var i = 0; i < requestNames.Length; i++)
        {
            var index = requestTypeInput[i];
            requestNames[i] = operation.Parameters[index].Name;
        }

        return new MessageProxy(requestNames, requestType);
    }

    private static MessageProxy CreateResponseProxy(MessageAssembler operation)
    {
        Type responseType;
        string[] names;
        if (operation.OperationType == MethodType.Unary || operation.OperationType == MethodType.ClientStreaming)
        {
            responseType = operation.ResponseType;
            names = operation.ResponseType == typeof(Message) ? Array.Empty<string>() : MessageProxy.UnaryResultNames;
        }
        else
        {
            responseType = operation.HeaderResponseType ?? typeof(Message);
            names = operation.GetResponseHeaderNames();
        }

        return new MessageProxy(names, responseType);
    }

    private static StreamProxy? CreateRequestStreamProxy(MessageAssembler operation)
    {
        if (operation.OperationType != MethodType.ClientStreaming && operation.OperationType != MethodType.DuplexStreaming)
        {
            return null;
        }

        return new StreamProxy(operation.RequestType.GenericTypeArguments[0]);
    }

    private static StreamProxy? CreateResponseStreamProxy(MessageAssembler operation)
    {
        if (operation.OperationType != MethodType.ServerStreaming && operation.OperationType != MethodType.DuplexStreaming)
        {
            return null;
        }

        return new StreamProxy(operation.ResponseType.GenericTypeArguments[0]);
    }
}