// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Generator.Internal;

[DebuggerDisplay("{OperationType} {OperationName}")]
internal sealed class OperationDescription
{
    public OperationDescription(IMethodSymbol method, string serviceName, string operationName)
    {
        ServiceName = serviceName;
        Method = new MethodDescription(method);
        ValidateSignature();

        OperationName = operationName;
        (ResponseType, ResponseTypeIndex, HeaderResponseType, HeaderResponseTypeInput) = CreateResponseType(method.ReturnType);
        (RequestType, RequestTypeInput, HeaderRequestType, HeaderRequestTypeInput) = GetRequestType();
        OperationType = GetOperationType();
        ContextInput = GetContextInput();
        IsAsync = SyntaxTools.IsTask(method.ReturnType);
        GrpcMethodName = "Method" + OperationName;
        GrpcMethodInputHeaderName = "MethodInputHeader" + OperationName;
        GrpcMethodOutputHeaderName = "MethodOutputHeader" + OperationName;
    }

    public MethodDescription Method { get; }

    public string ServiceName { get; }

    public string OperationName { get; }

    public MessageDescription ResponseType { get; }

    public int ResponseTypeIndex { get; }

    public MessageDescription? HeaderResponseType { get; }

    public int[] HeaderResponseTypeInput { get; }

    public MessageDescription RequestType { get; }

    public int[] RequestTypeInput { get; }

    public MessageDescription? HeaderRequestType { get; }

    public int[] HeaderRequestTypeInput { get; }

    public int[] ContextInput { get; }

    public MethodType OperationType { get; }

    public bool IsAsync { get; }

    public string GrpcMethodName { get; }

    public string GrpcMethodInputHeaderName { get; }

    public string GrpcMethodOutputHeaderName { get; }

    // implemented only for unary calls
    public bool IsCompatibleWith(OperationDescription other)
    {
        if (OperationType != other.OperationType
            || RequestType.Properties.Length != other.RequestType.Properties.Length
            || ResponseType.Properties.Length != other.ResponseType.Properties.Length)
        {
            return false;
        }

        for (var i = 0; i < RequestType.Properties.Length; i++)
        {
            var x = RequestType.Properties[i];
            var y = other.RequestType.Properties[i];
            if (!x.Equals(y, StringComparison.Ordinal))
            {
                return false;
            }
        }

        for (var i = 0; i < ResponseType.Properties.Length; i++)
        {
            var x = ResponseType.Properties[i];
            var y = other.ResponseType.Properties[i];
            if (!x.Equals(y, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsContextParameter(ITypeSymbol type)
    {
        return type.IsAssignableFrom(typeof(ServerCallContext))
               || type.Is(typeof(CancellationToken))
               || type.Is(typeof(CancellationToken?))
               || type.Is(typeof(CallContext))
               || type.Is(typeof(CallOptions))
               || type.Is(typeof(CallOptions?));
    }

    private static bool IsDataParameter(ITypeSymbol type)
    {
        return !SyntaxTools.IsTask(type)
               && !IsContextParameter(type)
               && !SyntaxTools.IsStream(type);
    }

    private static MessageDescription CreateMessage(params ITypeSymbol[] properties)
    {
        return new MessageDescription(properties.Select(SyntaxTools.GetFullName).ToArray());
    }

    private (MessageDescription ResponseType, int Index, MessageDescription? HeaderType, int[] HeaderIndexes) CreateResponseType(ITypeSymbol returnType)
    {
        if (SyntaxTools.IsVoid(returnType))
        {
            return (MessageDescription.Empty, 0, null, Array.Empty<int>());
        }

        var responseType = returnType;
        if (SyntaxTools.IsTask(returnType))
        {
            var genericArguments = responseType.GenericTypeArguments();
            if (genericArguments.IsEmpty)
            {
                return (MessageDescription.Empty, 0, null, Array.Empty<int>());
            }

            responseType = genericArguments[0];
        }

        if (SyntaxTools.IsValueTuple(responseType) && responseType.GenericTypeArguments().Any(SyntaxTools.IsAsyncEnumerable))
        {
            if (!SyntaxTools.IsTask(returnType))
            {
                ThrowInvalidSignature("Wrap return type with Task<> or ValueTask<>.");
            }

            var genericArguments = responseType.GenericTypeArguments();
            if (genericArguments.Length == 1)
            {
                ThrowInvalidSignature("Unwrap return type from ValueTuple<>.");
            }

            var streamIndex = -1;
            var headerIndexes = new List<int>();
            var headerTypes = new List<ITypeSymbol>();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var genericArgument = genericArguments[i];
                if (SyntaxTools.IsAsyncEnumerable(genericArgument))
                {
                    responseType = genericArgument.GenericTypeArguments()[0];
                    if (streamIndex >= 0 || IsContextParameter(responseType) || !IsDataParameter(responseType))
                    {
                        ThrowInvalidSignature();
                    }

                    streamIndex = i;
                }
                else if (IsContextParameter(genericArgument) || !IsDataParameter(genericArgument))
                {
                    ThrowInvalidSignature();
                }
                else
                {
                    headerIndexes.Add(i);
                    headerTypes.Add(genericArgument);
                }
            }

            return (
                CreateMessage(responseType),
                streamIndex,
                CreateMessage(headerTypes.ToArray()),
                headerIndexes.ToArray());
        }

        if (SyntaxTools.IsAsyncEnumerable(responseType))
        {
            responseType = responseType.GenericTypeArguments()[0];
        }

        if (IsContextParameter(responseType) || !IsDataParameter(responseType))
        {
            ThrowInvalidSignature();
        }

        return (CreateMessage(responseType), 0, null, Array.Empty<int>());
    }

    private (MessageDescription RequestType, int[] DataIndexes, MessageDescription? HeaderType, int[] HeaderIndexes) GetRequestType()
    {
        if (Method.Parameters.Length == 0)
        {
            return (MessageDescription.Empty, Array.Empty<int>(), null, Array.Empty<int>());
        }

        var dataParameters = new List<ITypeSymbol>();
        var dataParameterIndexes = new List<int>();
        var streamingIndex = -1;

        for (var i = 0; i < Method.Parameters.Length; i++)
        {
            var parameter = Method.Parameters[i];
            if (IsDataParameter(parameter.TypeSymbol))
            {
                if (SyntaxTools.IsAsyncEnumerable(parameter.TypeSymbol))
                {
                    streamingIndex = i;
                }
                else
                {
                    dataParameters.Add(parameter.TypeSymbol);
                    dataParameterIndexes.Add(i);
                }
            }
        }

        if (streamingIndex >= 0)
        {
            var requestType = CreateMessage(Method.Parameters[streamingIndex].TypeSymbol.GenericTypeArguments()[0]);
            MessageDescription? headerType = null;
            if (dataParameters.Count > 0)
            {
                headerType = CreateMessage(dataParameters.ToArray());
            }

            return (
                requestType,
                new[] { streamingIndex },
                headerType,
                dataParameterIndexes.ToArray());
        }

        return (
            CreateMessage(dataParameters.ToArray()),
            dataParameterIndexes.ToArray(),
            null,
            Array.Empty<int>());
    }

    private int[] GetContextInput()
    {
        if (Method.Parameters.Length == 0)
        {
            return Array.Empty<int>();
        }

        var indexes = new List<int>();

        for (var i = 0; i < Method.Parameters.Length; i++)
        {
            if (IsContextParameter(Method.Parameters[i].TypeSymbol))
            {
                indexes.Add(i);
            }
        }

        return indexes.Count == 0 ? Array.Empty<int>() : indexes.ToArray();
    }

    private MethodType GetOperationType()
    {
        var returnType = Method.ReturnTypeSymbol;
        if (SyntaxTools.IsTask(Method.ReturnTypeSymbol))
        {
            var args = returnType.GenericTypeArguments();
            returnType = args.IsEmpty ? returnType : args[0];
        }

        var responseIsStreaming = SyntaxTools.IsAsyncEnumerable(returnType)
                                  || (SyntaxTools.IsValueTuple(returnType) && returnType.GenericTypeArguments().Any(SyntaxTools.IsAsyncEnumerable));

        var requestIsStreaming = Method.Parameters.Select(i => i.TypeSymbol).Any(SyntaxTools.IsAsyncEnumerable);
        if (responseIsStreaming)
        {
            return requestIsStreaming ? MethodType.DuplexStreaming : MethodType.ServerStreaming;
        }

        return requestIsStreaming ? MethodType.ClientStreaming : MethodType.Unary;
    }

    private void ValidateSignature()
    {
        if (Method.TypeArguments.Length != 0)
        {
            ThrowInvalidSignature();
        }

        var hasInputStreaming = false;

        for (var i = 0; i < Method.Parameters.Length; i++)
        {
            var parameter = Method.Parameters[i];

            if (parameter.IsOut || parameter.IsRef)
            {
                ThrowInvalidSignature();
            }

            if (IsDataParameter(parameter.TypeSymbol))
            {
                if (SyntaxTools.IsAsyncEnumerable(parameter.TypeSymbol))
                {
                    if (hasInputStreaming)
                    {
                        ThrowInvalidSignature();
                    }

                    hasInputStreaming = true;
                }
            }
            else if (!IsContextParameter(parameter.TypeSymbol))
            {
                ThrowInvalidSignature();
            }
        }
    }

    private void ThrowInvalidSignature(string? additionalInfo = null)
    {
        var message = new StringBuilder()
            .AppendFormat("Method signature [{0}] is not supported.", SyntaxTools.GetSignature(Method.Source));

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            message.Append(" ").Append(additionalInfo);
        }

        throw new NotSupportedException(message.ToString());
    }
}