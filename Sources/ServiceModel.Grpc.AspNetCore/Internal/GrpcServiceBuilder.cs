using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class GrpcServiceBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private readonly Type _contractType;

        private Type _serviceType;

        public GrpcServiceBuilder(Type contractType)
        {
            _contractType = contractType;

            _typeBuilder = ProxyAssembly
                .DefaultModule
                .DefineType(
                    "{0}.{1}Service".FormatWith(ReflectionTools.GetNamespace(contractType), contractType.Name),
                    TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Class | TypeAttributes.Sealed);
        }

        public void BuildCall(MessageAssembler message)
        {
            if (message.OperationType == MethodType.ClientStreaming)
            {
                BuildClientStreamingCall(message);
                return;
            }

            if (message.OperationType == MethodType.DuplexStreaming)
            {
                BuildDuplexStreamingCall(message);
                return;
            }

            MethodBuilder method;
            int methodParametersCount;
            if (message.OperationType == MethodType.Unary)
            {
                // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
                methodParametersCount = 3;
                method = _typeBuilder
                    .DefineMethod(
                        message.Operation.Name,
                        MethodAttributes.Public | MethodAttributes.Static,
                        typeof(Task<>).MakeGenericType(message.ResponseType),
                        new[] { _contractType, message.RequestType, typeof(ServerCallContext) });
            }
            else if (message.OperationType == MethodType.ServerStreaming)
            {
                // Task Invoke(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
                methodParametersCount = 4;
                method = _typeBuilder
                    .DefineMethod(
                        message.Operation.Name,
                        MethodAttributes.Public | MethodAttributes.Static,
                        typeof(Task),
                        new[] { _contractType, message.RequestType, typeof(IServerStreamWriter<>).MakeGenericType(message.ResponseType), typeof(ServerCallContext) });
            }
            else
            {
                throw new NotImplementedException();
            }

            var body = method.GetILGenerator();

            if (!IsSignatureSupported(message, body))
            {
                return;
            }

            // service
            body.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    // ServerChannelAdapter.GetContext(context)
                    body.EmitLdarg(methodParametersCount - 1);
                    body.Emit(OpCodes.Call, GetContext(parameter.ParameterType));
                }
                else
                {
                    var propertyName = "Value" + (Array.IndexOf(message.RequestTypeInput, i) + 1);

                    // request.Value1
                    body.Emit(OpCodes.Ldarg_1);
                    body.Emit(OpCodes.Callvirt, message.RequestType.InstanceProperty(propertyName).GetMethod);
                }
            }

            // service.Method
            body.Emit(OpCodes.Callvirt, _contractType.InstanceMethod(message.Operation.Name));

            if (message.OperationType == MethodType.Unary)
            {
                if (message.IsAsync)
                {
                    if (message.ResponseType.IsGenericType)
                    {
                        // ServerChannelAdapter.GetUnaryCallResult
                        body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.GetUnaryCallResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));
                    }
                    else
                    {
                        // ServerChannelAdapter.UnaryCallWait
                        body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.UnaryCallWait)));
                    }
                }
                else
                {
                    if (message.ResponseType.IsGenericType)
                    {
                        // new Message<T>
                        body.Emit(OpCodes.Newobj, message.ResponseType.Constructor(message.ResponseType.GenericTypeArguments));
                    }
                    else
                    {
                        // new Message
                        body.Emit(OpCodes.Newobj, message.ResponseType.Constructor());
                    }

                    // Task.FromResult
                    body.Emit(OpCodes.Call, typeof(Task).StaticMethod(nameof(Task.FromResult)).MakeGenericMethod(message.ResponseType));
                }
            }
            else if (message.OperationType == MethodType.ServerStreaming)
            {
                // ServerChannelAdapter.WriteServerStreamingResult(result, stream, serverCallContext);
                body.Emit(OpCodes.Ldarg_2); // stream
                body.Emit(OpCodes.Ldarg_3); // serverCallContext
                if (message.ResponseType.IsGenericType)
                {
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.WriteServerStreamingResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));
                }
                else
                {
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.WriteServerStreamingResult)).MakeGenericMethod(message.ResponseType));
                }
            }

            body.Emit(OpCodes.Ret);
        }

        public TMethod CreateCall<TMethod>(string methodName)
            where TMethod : Delegate
        {
            if (_serviceType == null)
            {
                _serviceType = _typeBuilder.CreateType();
            }

            return (TMethod)_serviceType.StaticMethod(methodName).CreateDelegate(typeof(TMethod));
        }

        private static MethodInfo GetContext(Type returnType)
        {
            MethodInfo method = null;
            try
            {
                method = typeof(ServerChannelAdapter).StaticMethodByReturnType(nameof(ServerChannelAdapter.GetContext), returnType);
            }
            catch (ArgumentOutOfRangeException)
            {
                // method not found
            }

            return method;
        }

        private bool IsSignatureSupported(MessageAssembler message, ILGenerator body)
        {
            if (message.ContextInput.Any(i => GetContext(message.Parameters[i].ParameterType) == null))
            {
                // throw new NotSupportedException("Signature is not supported.");
                body.Emit(OpCodes.Ldstr, "Method {0}.{1}.{2} signature is not supported.".FormatWith(ReflectionTools.GetNamespace(_contractType), _contractType.Name, message.Operation.Name));
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }));
                body.Emit(OpCodes.Throw);
                return false;
            }

            return true;
        }

        private void BuildClientStreamingCall(MessageAssembler message)
        {
            var body = _typeBuilder
                .DefineMethod(
                    message.Operation.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(Task<>).MakeGenericType(message.ResponseType),
                    new[] { _contractType, typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType), typeof(ServerCallContext) })
                .GetILGenerator();

            if (!IsSignatureSupported(message, body))
            {
                return;
            }

            // service
            body.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    // ServerChannelAdapter.GetContext(context)
                    body.Emit(OpCodes.Ldarg_2);
                    body.Emit(OpCodes.Call, GetContext(parameter.ParameterType));
                }
                else
                {
                    // ReadClientStream()
                    body.Emit(OpCodes.Ldarg_1); // stream
                    body.Emit(OpCodes.Ldarg_2); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            body.Emit(OpCodes.Callvirt, _contractType.InstanceMethod(message.Operation.Name));

            if (message.ResponseType.IsGenericType)
            {
                // ServerChannelAdapter.GetUnaryCallResult
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.GetUnaryCallResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));
            }
            else
            {
                // ServerChannelAdapter.UnaryCallWait
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.UnaryCallWait)));
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreamingCall(MessageAssembler message)
        {
            var body = _typeBuilder
                .DefineMethod(
                    message.Operation.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(Task<>).MakeGenericType(message.ResponseType),
                    new[]
                    {
                        _contractType,
                        typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType),
                        typeof(IServerStreamWriter<>).MakeGenericType(message.ResponseType),
                        typeof(ServerCallContext)
                    })
                .GetILGenerator();

            if (!IsSignatureSupported(message, body))
            {
                return;
            }

            body.Emit(OpCodes.Ldarg_0); // service

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    // ServerChannelAdapter.GetContext(context)
                    body.Emit(OpCodes.Ldarg_3);
                    body.Emit(OpCodes.Call, GetContext(parameter.ParameterType));
                }
                else
                {
                    // ReadClientStream()
                    body.Emit(OpCodes.Ldarg_1); // input
                    body.Emit(OpCodes.Ldarg_3); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            body.Emit(OpCodes.Callvirt, _contractType.InstanceMethod(message.Operation.Name));

            // ServerChannelAdapter.WriteServerStreamingResult
            body.Emit(OpCodes.Ldarg_2); // output
            body.Emit(OpCodes.Ldarg_3); // context
            body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.WriteServerStreamingResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments));

            body.Emit(OpCodes.Ret);
        }
    }
}
