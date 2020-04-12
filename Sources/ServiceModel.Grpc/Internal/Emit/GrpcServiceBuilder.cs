using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class GrpcServiceBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private readonly Type _contractType;

        public GrpcServiceBuilder(Type contractType)
        {
            _contractType = contractType;

            _typeBuilder = ProxyAssembly
                .DefaultModule
                .DefineType(
                    "{0}.{1}Service".FormatWith(ReflectionTools.GetNamespace(contractType), contractType.Name),
                    TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Class | TypeAttributes.Sealed);
        }

        public void BuildNotSupportedCall(MessageAssembler message, string error)
        {
            var body = CreateMethodWithSignature(message);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        public void BuildCall(MessageAssembler message)
        {
            var body = CreateMethodWithSignature(message);

            switch (message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, message);
                    break;

                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, message);
                    break;

                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, message);
                    break;

                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, message);
                    break;
            }
        }

        public Type BuildType()
        {
            return _typeBuilder.CreateTypeInfo();
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

        private void BuildUnary(ILGenerator body, MessageAssembler message)
        {
            // service
            body.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    // ServerChannelAdapter.GetContext(context)
                    body.EmitLdarg(2);
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

            body.Emit(OpCodes.Ret);
        }

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message)
        {
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

        private void BuildServerStreaming(ILGenerator body, MessageAssembler message)
        {
            // service
            body.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    // ServerChannelAdapter.GetContext(context)
                    body.EmitLdarg(3);
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

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message)
        {
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

        private ILGenerator CreateMethodWithSignature(MessageAssembler message)
        {
            switch (message.OperationType)
            {
                case MethodType.Unary:
                    // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            message.Operation.Name,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { _contractType, message.RequestType, typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ClientStreaming:
                    // Task<TResponse> Invoke(TService service, IAsyncStreamReader<TRequest> request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            message.Operation.Name,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { _contractType, typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ServerStreaming:
                    // Task Invoke(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            message.Operation.Name,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task),
                            new[] { _contractType, message.RequestType, typeof(IServerStreamWriter<>).MakeGenericType(message.ResponseType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.DuplexStreaming:
                    // Task Invoke(TService service, IAsyncStreamReader<TRequest> request, IServerStreamWriter<TResponse> response, ServerCallContext context)
                    return _typeBuilder
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
            }

            throw new NotImplementedException("{0} operation is not implemented.".FormatWith(message.OperationType));
        }
    }
}
