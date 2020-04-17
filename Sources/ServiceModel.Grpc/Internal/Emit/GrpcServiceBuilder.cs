using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class GrpcServiceBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private readonly Type _contractType;
        private readonly IMarshallerFactory _marshallerFactory;
        private readonly ILGenerator _initializeHeadersMarshallerMethod;

        public GrpcServiceBuilder(Type contractType, IMarshallerFactory marshallerFactory)
        {
            _contractType = contractType;
            _marshallerFactory = marshallerFactory;

            _typeBuilder = ProxyAssembly
                .DefaultModule
                .DefineType(
                    "{0}.{1}Service".FormatWith(ReflectionTools.GetNamespace(contractType), contractType.Name),
                    TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Class | TypeAttributes.Sealed);

            _initializeHeadersMarshallerMethod = _typeBuilder
                .DefineMethod(
                    "InitializeHeadersMarshaller",
                    MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public,
                    typeof(void),
                    new[] { typeof(IMarshallerFactory) })
                .GetILGenerator();
        }

        public void BuildNotSupportedCall(MessageAssembler message, string methodName, string error)
        {
            var body = CreateMethodWithSignature(message, methodName);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        public void BuildCall(MessageAssembler message, string methodName)
        {
            var body = CreateMethodWithSignature(message, methodName);
            var headersMarshallerFiled = InitializeHeadersMarshaller(message);

            switch (message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, message);
                    break;

                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, message, headersMarshallerFiled);
                    break;

                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, message);
                    break;

                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, message, headersMarshallerFiled);
                    break;
            }
        }

        public Type BuildType()
        {
            _initializeHeadersMarshallerMethod.Emit(OpCodes.Ret);

            var type = _typeBuilder.CreateTypeInfo();

            var defineGrpcMethod = (Action<IMarshallerFactory>)type
                .StaticMethod("InitializeHeadersMarshaller")
                .CreateDelegate(typeof(Action<IMarshallerFactory>));
            defineGrpcMethod(_marshallerFactory);

            return type;
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
                    PushContext(body, 2, parameter.ParameterType);
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
                AdaptSyncUnaryCallResult(body, message);
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

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message, FieldBuilder headersMarshallerFiled)
        {
            DeclareHeaderValues(body, message, headersMarshallerFiled, 2);

            // service
            body.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    PushContext(body, 2, parameter.ParameterType);
                }
                else if (message.HeaderRequestTypeInput.Contains(i))
                {
                    PushHeaderProperty(body, message, i);
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

            AdaptSyncUnaryCallResult(body, message);

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
                    PushContext(body, 3, parameter.ParameterType);
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

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message, FieldBuilder headersMarshallerFiled)
        {
            DeclareHeaderValues(body, message, headersMarshallerFiled, 3);

            body.Emit(OpCodes.Ldarg_0); // service

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    PushContext(body, 3, parameter.ParameterType);
                }
                else if (message.HeaderRequestTypeInput.Contains(i))
                {
                    PushHeaderProperty(body, message, i);
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

        private ILGenerator CreateMethodWithSignature(MessageAssembler message, string methodName)
        {
            switch (message.OperationType)
            {
                case MethodType.Unary:
                    // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { _contractType, message.RequestType, typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ClientStreaming:
                    // Task<TResponse> Invoke(TService service, IAsyncStreamReader<TRequest> request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { _contractType, typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ServerStreaming:
                    // Task Invoke(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.Static,
                            typeof(Task),
                            new[] { _contractType, message.RequestType, typeof(IServerStreamWriter<>).MakeGenericType(message.ResponseType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.DuplexStreaming:
                    // Task Invoke(TService service, IAsyncStreamReader<TRequest> request, IServerStreamWriter<TResponse> response, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
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

        private void AdaptSyncUnaryCallResult(ILGenerator body, MessageAssembler message)
        {
            if (message.ResponseType.IsGenericType)
            {
                var adapter = typeof(ServerChannelAdapter)
                    .StaticMethod(message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.GetUnaryCallResultValueTask) : nameof(ServerChannelAdapter.GetUnaryCallResultTask))
                    .MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]);

                // ServerChannelAdapter.GetUnaryCallResult
                body.Emit(OpCodes.Call, adapter);
            }
            else
            {
                var adapter = typeof(ServerChannelAdapter)
                    .StaticMethod(message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.UnaryCallWaitValueTask) : nameof(ServerChannelAdapter.UnaryCallWaitTask));

                // ServerChannelAdapter.UnaryCallWait
                body.Emit(OpCodes.Call, adapter);
            }
        }

        private void PushContext(ILGenerator body, int serverContextParameterIndex, Type contextType)
        {
            // ServerChannelAdapter.GetContext(context)
            body.EmitLdarg(serverContextParameterIndex);
            body.Emit(OpCodes.Call, ContractDescription.GetServiceContextOption(contextType));
        }

        private void PushHeaderProperty(ILGenerator body, MessageAssembler message, int parameterIndex)
        {
            var propertyName = "Value" + (Array.IndexOf(message.HeaderRequestTypeInput, parameterIndex) + 1);
            body.Emit(OpCodes.Ldloc_0); // headers
            body.Emit(OpCodes.Callvirt, message.HeaderRequestType.InstanceProperty(propertyName).GetMethod); // headers.Value1
        }

        private void DeclareHeaderValues(ILGenerator body, MessageAssembler message, FieldBuilder headersMarshallerFiled, int contextParameterIndex)
        {
            if (headersMarshallerFiled != null)
            {
                body.DeclareLocal(message.HeaderRequestType); // var headers

                body.Emit(OpCodes.Ldsfld, headersMarshallerFiled); // static Marshaller<>
                body.EmitLdarg(contextParameterIndex); // context
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.GetMethodInputHeader)).MakeGenericMethod(message.HeaderRequestType));
                body.Emit(OpCodes.Stloc_0);
            }
        }

        private FieldBuilder InitializeHeadersMarshaller(MessageAssembler message)
        {
            if (message.HeaderRequestType == null)
            {
                return null;
            }

            var filedType = typeof(Marshaller<>).MakeGenericType(message.HeaderRequestType);

            // private static Marshaller<Message<string, string>> ConcatBHeadersMarshaller;
            var field = _typeBuilder
                .DefineField(
                    "{0}-{1}-HeadersMarshaller".FormatWith(_contractType.Name, message.Operation.Name),
                    filedType,
                    FieldAttributes.Private | FieldAttributes.Static);

            var createMarshaller = typeof(IMarshallerFactory).InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller));

            _initializeHeadersMarshallerMethod.Emit(OpCodes.Ldarg_0);
            _initializeHeadersMarshallerMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.HeaderRequestType));
            _initializeHeadersMarshallerMethod.Emit(OpCodes.Stsfld, field);

            return field;
        }
    }
}
