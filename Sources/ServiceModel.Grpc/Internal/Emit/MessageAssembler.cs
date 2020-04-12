using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Grpc.Core;
using ServiceModel.Grpc.Channel;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class MessageAssembler
    {
        public MessageAssembler(MethodInfo operation)
        {
            Operation = operation;
            Parameters = operation.GetParameters();

            ValidateSignature();

            ResponseType = GetResponseType();
            (RequestType, RequestTypeInput) = GetRequestType();
            ContextInput = GetContextInput();
            OperationType = GetOperationType();
            IsAsync = ReflectionTools.IsTask(Operation.ReturnType);
        }

        public MethodInfo Operation { get; }

        public ParameterInfo[] Parameters { get; }

        public Type ResponseType { get; }

        public Type RequestType { get; }

        public int[] RequestTypeInput { get; }

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

        private (Type, int[]) GetRequestType()
        {
            if (Parameters.Length == 0)
            {
                return (typeof(Message), Array.Empty<int>());
            }

            var dataParameters = new List<Type>();
            var dataParameterIndexes = new List<int>();
            var hasStreaming = false;

            for (var i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];
                if (IsDataParameter(parameter.ParameterType))
                {
                    dataParameters.Add(parameter.ParameterType);
                    dataParameterIndexes.Add(i);

                    if (!hasStreaming && ReflectionTools.IsAsyncEnumerable(parameter.ParameterType))
                    {
                        hasStreaming = true;
                    }
                }
            }

            if (dataParameters.Count == 0)
            {
                return (typeof(Message), Array.Empty<int>());
            }

            if (hasStreaming)
            {
                if (dataParameters.Count != 1)
                {
                    ThrowInvalidSignature();
                }

                dataParameters[0] = dataParameters[0].GenericTypeArguments[0];
            }

            // ServiceModel.Grpc.Channel.Message`2
            Type requestType;
            try
            {
                requestType = typeof(Message).Assembly.GetType(typeof(Message).FullName + "`" + dataParameters.Count, true, false);
            }
            catch (Exception ex)
            {
                ThrowInvalidSignature(ex);
                throw;
            }

            requestType = requestType.MakeGenericType(dataParameters.ToArray());
            return (requestType, dataParameterIndexes.ToArray());
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
            var requestIsStreaming = RequestTypeInput.Length == 1 && ReflectionTools.IsAsyncEnumerable(Parameters[RequestTypeInput[0]].ParameterType);

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

            for (var i = 0; i < Parameters.Length; i++)
            {
                var parameter = Parameters[i];

                if (parameter.IsOut() || parameter.IsRef())
                {
                    ThrowInvalidSignature();
                }

                if (!IsDataParameter(parameter.ParameterType) && !IsContextParameter(parameter.ParameterType))
                {
                    ThrowInvalidSignature();
                }
            }
        }

        private void ThrowInvalidSignature(Exception ex = default)
        {
            var message = "Method signature [{0}] is not supported.".FormatWith(Operation);

            if (ex == null)
            {
                throw new NotSupportedException(message);
            }

            throw new NotSupportedException(message, ex);
        }
    }
}
