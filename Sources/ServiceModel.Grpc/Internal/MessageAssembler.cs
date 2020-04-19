using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            ResponseType = GetResponseType();
            (RequestType, RequestTypeInput, HeaderRequestType, HeaderRequestTypeInput) = GetRequestType();
            ContextInput = GetContextInput();
            OperationType = GetOperationType();
            IsAsync = ReflectionTools.IsTask(Operation.ReturnType);
        }

        public MethodInfo Operation { get; }

        public ParameterInfo[] Parameters { get; }

        public Type ResponseType { get; }

        public Type RequestType { get; }

        public int[] RequestTypeInput { get; }

        public Type HeaderRequestType { get; }

        public int[] HeaderRequestTypeInput { get; }

        public MethodType OperationType { get; }

        public int[] ContextInput { get; }

        public bool IsAsync { get; }

        private static bool IsContextParameter(Type type)
        {
            return typeof(ServerCallContext).IsAssignableFrom(type)
                || typeof(CancellationToken) == type
                || typeof(CallContext) == type
                || typeof(CallOptions) == type;
        }

        private static bool IsDataParameter(Type type)
        {
            return !ReflectionTools.IsTask(type)
                && !IsContextParameter(type)
                && !ReflectionTools.IsStream(type);
        }

        private Type GetResponseType()
        {
            var returnType = Operation.ReturnType;
            if (returnType == typeof(void))
            {
                return typeof(Message);
            }

            var responseType = returnType;

            if (ReflectionTools.IsTask(returnType))
            {
                if (!returnType.IsGenericType)
                {
                    return typeof(Message);
                }

                responseType = returnType.GenericTypeArguments[0];
            }

            if (ReflectionTools.IsAsyncEnumerable(responseType))
            {
                responseType = returnType.GenericTypeArguments[0];
            }

            if (IsContextParameter(responseType) || !IsDataParameter(responseType))
            {
                ThrowInvalidSignature();
            }

            return typeof(Message<>).MakeGenericType(responseType);
        }

        private (Type, int[], Type, int[]) GetRequestType()
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
            var responseIsStreaming = ReflectionTools.IsAsyncEnumerable(Operation.ReturnType);
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

        private void ThrowInvalidSignature(Exception ex = default)
        {
            var message = "Method signature [{0}] is not supported.".FormatWith(ReflectionTools.GetSignature(Operation));

            if (ex == null)
            {
                throw new NotSupportedException(message);
            }

            throw new NotSupportedException(message, ex);
        }
    }
}
