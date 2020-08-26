// <copyright>
// Copyright 2020 Max Ieremenko
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
using System.Linq;
using System.Threading;
using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace ServiceModel.Grpc.DesignTime.Internal
{
    internal sealed class OperationDescription
    {
        public OperationDescription(IMethodSymbol method, string serviceName)
        {
            ServiceName = serviceName;
            Method = new MethodDescription(method);
            ValidateSignature();

            OperationName = ServiceContract.GetServiceOperationName(method);
            ResponseType = CreateResponseType(method.ReturnType);
            (RequestType, RequestTypeInput, HeaderRequestType, HeaderRequestTypeInput) = GetRequestType();
            OperationType = GetOperationType();
            ContextInput = GetContextInput();
            IsAsync = SyntaxTools.IsTask(method.ReturnType);
            GrpcMethodName = "Method" + OperationName;
            GrpcMethodHeaderName = "MethodHeader" + OperationName;
        }

        public MethodDescription Method { get; }

        public string ServiceName { get; }

        public string OperationName { get; }

        public MessageDescription ResponseType { get; }

        public MessageDescription RequestType { get; }

        public int[] RequestTypeInput { get; }

        public MessageDescription? HeaderRequestType { get; }

        public int[] HeaderRequestTypeInput { get; }

        public int[] ContextInput { get; }

        public MethodType OperationType { get; }

        public bool IsAsync { get; }

        public string GrpcMethodName { get; }

        public string GrpcMethodHeaderName { get; }

        private static bool IsContextParameter(ITypeSymbol type)
        {
            return type.IsAssignableFrom(typeof(ServerCallContext))
                   || type.Is(typeof(CancellationToken))
                   || type.Is(typeof(CallContext))
                   || type.Is(typeof(CallOptions));
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

        private MessageDescription CreateResponseType(ITypeSymbol returnType)
        {
            if (SyntaxTools.IsVoid(returnType))
            {
                return MessageDescription.Empty();
            }

            var responseType = returnType;
            if (SyntaxTools.IsTask(returnType))
            {
                var genericArguments = responseType.GenericTypeArguments();
                if (genericArguments.IsEmpty)
                {
                    return MessageDescription.Empty();
                }

                responseType = genericArguments[0];
            }

            if (SyntaxTools.IsAsyncEnumerable(responseType))
            {
                responseType = responseType.GenericTypeArguments()[0];
            }

            if (IsContextParameter(responseType) || !IsDataParameter(responseType))
            {
                ThrowInvalidSignature();
            }

            return new MessageDescription(new[] { SyntaxTools.GetFullName(responseType) });
        }

        private (MessageDescription RequestType, int[] DataIndexes, MessageDescription? HeaderType, int[] HeaderIndexes) GetRequestType()
        {
            if (Method.Parameters.Length == 0)
            {
                return (MessageDescription.Empty(), Array.Empty<int>(), null, Array.Empty<int>());
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
            var returnTypeGenericArgs = Method.ReturnTypeSymbol.GenericTypeArguments();
            var responseIsStreaming = SyntaxTools.IsAsyncEnumerable(Method.ReturnTypeSymbol)
                                      || (!returnTypeGenericArgs.IsEmpty && SyntaxTools.IsTask(Method.ReturnTypeSymbol) && SyntaxTools.IsAsyncEnumerable(returnTypeGenericArgs[0]));
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

        private void ThrowInvalidSignature()
        {
            var message = "Method signature [{0}] is not supported.".FormatWith(SyntaxTools.GetSignature(Method.Source));
            throw new NotSupportedException(message);
        }
    }
}
