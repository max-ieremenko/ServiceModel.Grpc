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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class MessageAssembler
    {
        public MessageAssembler(MethodInfo operation)
        {
            Operation = operation;
            Parameters = operation.GetParameters();

            ValidateSignature();

            (ResponseType, ResponseTypeIndex, HeaderResponseType, HeaderResponseTypeInput) = GetResponseType(operation.ReturnType);
            (RequestType, RequestTypeInput, HeaderRequestType, HeaderRequestTypeInput) = GetRequestType();
            ContextInput = GetContextInput();
            OperationType = GetOperationType();
            IsAsync = ReflectionTools.IsTask(Operation.ReturnType);
        }

        public MethodInfo Operation { get; }

        public ParameterInfo[] Parameters { get; }

        public Type ResponseType { get; }

        public int ResponseTypeIndex { get; }

        public Type? HeaderResponseType { get; }

        public int[] HeaderResponseTypeInput { get; }

        public Type RequestType { get; }

        public int[] RequestTypeInput { get; }

        public Type? HeaderRequestType { get; }

        public int[] HeaderRequestTypeInput { get; }

        public MethodType OperationType { get; }

        public int[] ContextInput { get; }

        public bool IsAsync { get; }

        public string[] GetResponseHeaderNames()
        {
            if (HeaderResponseTypeInput.Length == 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[HeaderResponseTypeInput.Length];
            var names = Operation.ReturnParameter!.GetCustomAttribute<TupleElementNamesAttribute>()?.TransformNames;

            for (var i = 0; i < result.Length; i++)
            {
                var index = HeaderResponseTypeInput[i];

                string? name = null;
                if (names != null)
                {
                    name = names[index];
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = "Item{0}".FormatWith((i + 1).ToString(CultureInfo.InvariantCulture));
                }

                result[i] = name!;
            }

            return result;
        }

        private static bool IsContextParameter(Type type)
        {
            return typeof(ServerCallContext).IsAssignableFrom(type)
                || typeof(CancellationToken) == type
                || typeof(CancellationToken?) == type
                || typeof(CallContext) == type
                || typeof(CallOptions) == type
                || typeof(CallOptions?) == type;
        }

        private static bool IsDataParameter(Type type)
        {
            return !ReflectionTools.IsTask(type)
                && !IsContextParameter(type)
                && !ReflectionTools.IsStream(type);
        }

        private (Type ResponseType, int Index, Type? HeaderType, int[] HeaderIndexes) GetResponseType(Type returnType)
        {
            if (returnType == typeof(void))
            {
                return (typeof(Message), 0, null, Array.Empty<int>());
            }

            var responseType = returnType;
            if (ReflectionTools.IsTask(returnType))
            {
                if (!returnType.IsGenericType)
                {
                    return (typeof(Message), 0, null, Array.Empty<int>());
                }

                responseType = returnType.GenericTypeArguments[0];
            }

            if (ReflectionTools.IsValueTuple(responseType) && responseType.GenericTypeArguments.Any(ReflectionTools.IsAsyncEnumerable))
            {
                if (!ReflectionTools.IsTask(returnType))
                {
                    ThrowInvalidSignature("Wrap return type with Task<> or ValueTask<>.");
                }

                var genericArguments = responseType.GenericTypeArguments;
                if (genericArguments.Length == 1)
                {
                    ThrowInvalidSignature("Unwrap return type from ValueTuple<>.");
                }

                var streamIndex = -1;
                var headerIndexes = new List<int>();
                var headerTypes = new List<Type>();
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    var genericArgument = genericArguments[i];
                    if (ReflectionTools.IsAsyncEnumerable(genericArgument))
                    {
                        responseType = genericArgument.GenericTypeArguments[0];
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
                    MessageBuilder.GetMessageType(responseType),
                    streamIndex,
                    MessageBuilder.GetMessageType(headerTypes.ToArray()),
                    headerIndexes.ToArray());
            }

            if (ReflectionTools.IsAsyncEnumerable(responseType))
            {
                responseType = responseType.GenericTypeArguments[0];
            }

            if (IsContextParameter(responseType) || !IsDataParameter(responseType))
            {
                ThrowInvalidSignature();
            }

            return (MessageBuilder.GetMessageType(responseType), 0, null, Array.Empty<int>());
        }

        private (Type MessageType, int[] DataIndexes, Type? HeaderType, int[] HeaderIndexes) GetRequestType()
        {
            if (Parameters.Length == 0)
            {
                return (typeof(Message), Array.Empty<int>(), null, Array.Empty<int>());
            }

            var dataParameters = new List<Type>();
            var dataParameterIndexes = new List<int>();
            var streamingIndex = -1;

            for (var i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];
                if (IsDataParameter(parameter.ParameterType))
                {
                    if (ReflectionTools.IsAsyncEnumerable(parameter.ParameterType))
                    {
                        streamingIndex = i;
                    }
                    else
                    {
                        dataParameters.Add(parameter.ParameterType);
                        dataParameterIndexes.Add(i);
                    }
                }
            }

            if (streamingIndex >= 0)
            {
                return (
                    MessageBuilder.GetMessageType(Parameters[streamingIndex].ParameterType.GenericTypeArguments[0]),
                    new[] { streamingIndex },
                    dataParameters.Count > 0 ? MessageBuilder.GetMessageType(dataParameters.ToArray()) : null,
                    dataParameterIndexes.ToArray());
            }

            return (
                MessageBuilder.GetMessageType(dataParameters.ToArray()),
                dataParameterIndexes.ToArray(),
                null,
                Array.Empty<int>());
        }

        private int[] GetContextInput()
        {
            if (Parameters.Length == 0)
            {
                return Array.Empty<int>();
            }

            var indexes = new List<int>();

            for (var i = 0; i < Parameters.Length; i++)
            {
                if (IsContextParameter(Parameters[i].ParameterType))
                {
                    indexes.Add(i);
                }
            }

            return indexes.Count == 0 ? Array.Empty<int>() : indexes.ToArray();
        }

        private MethodType GetOperationType()
        {
            var returnType = Operation.ReturnType;
            if (ReflectionTools.IsTask(returnType))
            {
                var args = returnType.GenericTypeArguments;
                returnType = args.Length == 0 ? returnType : args[0];
            }

            var responseIsStreaming = ReflectionTools.IsAsyncEnumerable(returnType)
                                      || (ReflectionTools.IsValueTuple(returnType) && returnType.GenericTypeArguments.Any(ReflectionTools.IsAsyncEnumerable));

            var requestIsStreaming = Parameters.Select(i => i.ParameterType).Any(ReflectionTools.IsAsyncEnumerable);
            if (responseIsStreaming)
            {
                return requestIsStreaming ? MethodType.DuplexStreaming : MethodType.ServerStreaming;
            }

            return requestIsStreaming ? MethodType.ClientStreaming : MethodType.Unary;
        }

        private void ValidateSignature()
        {
            if (Operation.IsGenericMethod)
            {
                ThrowInvalidSignature();
            }

            var hasInputStreaming = false;

            for (var i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];

                if (parameter.IsOut() || parameter.IsRef())
                {
                    ThrowInvalidSignature();
                }

                if (IsDataParameter(parameter.ParameterType))
                {
                    if (ReflectionTools.IsAsyncEnumerable(parameter.ParameterType))
                    {
                        if (hasInputStreaming)
                        {
                            ThrowInvalidSignature();
                        }

                        hasInputStreaming = true;
                    }
                }
                else if (!IsContextParameter(parameter.ParameterType))
                {
                    ThrowInvalidSignature();
                }
            }
        }

        private void ThrowInvalidSignature(string? additionalInfo = null)
        {
            var message = new StringBuilder()
                .AppendFormat("Method signature [{0}] is not supported.", ReflectionTools.GetSignature(Operation));

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message.Append(" ").Append(additionalInfo);
            }

            throw new NotSupportedException(message.ToString());
        }
    }
}
