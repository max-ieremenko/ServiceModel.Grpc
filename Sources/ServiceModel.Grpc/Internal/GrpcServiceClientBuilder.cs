using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal
{
    internal sealed class GrpcServiceClientBuilder<TContract>
        where TContract : class
    {
        private readonly TypeBuilder _typeBuilder;

        public GrpcServiceClientBuilder()
        {
            var contractType = typeof(TContract);

            _typeBuilder = ProxyAssembly
                .DefaultModule
                .DefineType(
                    "{0}.{1}Client".FormatWith(ReflectionTools.GetNamespace(contractType), contractType.Name),
                    TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    typeof(ClientBase));
        }

        public Func<CallInvoker, TContract> Build(IMarshallerFactory marshallerFactory)
        {
            // ctor(CallInvoker callInvoker)
            BuildCtor();

            var defineGrpcMethod = _typeBuilder
                .DefineMethod(
                    "DefineGrpcMethods",
                    MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public,
                    typeof(void),
                    new[] { typeof(IMarshallerFactory) })
                .GetILGenerator();

            foreach (var interfaceType in ReflectionTools.ExpandInterface(typeof(TContract)))
            {
                _typeBuilder.AddInterfaceImplementation(interfaceType);

                foreach (var operation in ReflectionTools.GetMethods(interfaceType))
                {
                    var message = new MessageAssembler(operation);

                    var grpcMethodFiled = InitializeGrpcMethod(defineGrpcMethod, interfaceType, message);
                    BuildMethod(interfaceType, message, grpcMethodFiled);
                }
            }

            defineGrpcMethod.Emit(OpCodes.Ret);

            return CreateFactory(marshallerFactory);
        }

        private static MethodInfo GetCallOptionsCombine(MessageAssembler message)
        {
            var parameters = new Type[message.ContextInput.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = message.Parameters[message.ContextInput[i]].ParameterType;
            }

            MethodInfo method = null;
            try
            {
                method = typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.Combine), parameters);
            }
            catch (ArgumentOutOfRangeException)
            {
                // method not found
            }

            return method;
        }

        private Func<CallInvoker, TContract> CreateFactory(IMarshallerFactory marshallerFactory)
        {
            var type = _typeBuilder.CreateTypeInfo();

            var defineGrpcMethod = (Action<IMarshallerFactory>)type
                .StaticMethod("DefineGrpcMethods")
                .CreateDelegate(typeof(Action<IMarshallerFactory>));
            defineGrpcMethod(marshallerFactory);

            var callInvoker = Expression.Parameter(typeof(CallInvoker), "callInvoker");

            var ctor = Expression.New(type.Constructor(typeof(CallInvoker)), callInvoker);

            return Expression.Lambda<Func<CallInvoker, TContract>>(ctor, callInvoker).Compile();
        }

        private FieldBuilder InitializeGrpcMethod(ILGenerator defineGrpcMethod, Type interfaceType, MessageAssembler message)
        {
            if (!ServiceContract.IsServiceContractInterface(interfaceType) || !ServiceContract.IsServiceOperation(message.Operation))
            {
                return null;
            }

            var filedType = typeof(Method<,>).MakeGenericType(message.RequestType, message.ResponseType);

            // private static Method<string, string> ConcatBMethod;
            var field = _typeBuilder
                .DefineField(
                    "{0}-{1}".FormatWith(interfaceType.Name, message.Operation.Name),
                    filedType,
                    FieldAttributes.Private | FieldAttributes.Static);

            var createMarshaller = typeof(IMarshallerFactory).InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller));

            defineGrpcMethod.EmitLdcI4((int)message.OperationType); // MethodType
            defineGrpcMethod.Emit(OpCodes.Ldstr, ServiceContract.GetServiceName(interfaceType));
            defineGrpcMethod.Emit(OpCodes.Ldstr, ServiceContract.GetServiceOperationName(message.Operation));
            defineGrpcMethod.Emit(OpCodes.Ldarg_0);
            defineGrpcMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.RequestType));
            defineGrpcMethod.Emit(OpCodes.Ldarg_0);
            defineGrpcMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.ResponseType));
            defineGrpcMethod.Emit(OpCodes.Newobj, filedType.GetConstructor(new[]
            {
                typeof(MethodType),
                typeof(string),
                typeof(string),
                typeof(Marshaller<>).MakeGenericType(message.RequestType),
                typeof(Marshaller<>).MakeGenericType(message.ResponseType)
            }));
            defineGrpcMethod.Emit(OpCodes.Stsfld, field);

            return field;
        }

        private void BuildMethod(Type interfaceType, MessageAssembler message, FieldBuilder grpcMethodFiled)
        {
            var parameterTypes = message.Parameters.Select(i => i.ParameterType).ToArray();
            var method = _typeBuilder
                .DefineMethod(
                    message.Operation.Name,
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    message.Operation.ReturnType,
                    parameterTypes);

            _typeBuilder.DefineMethodOverride(method, message.Operation);

            var body = method.GetILGenerator();

            if (grpcMethodFiled == null)
            {
                // throw new NotSupportedException("Method is not operation contract.");
                body.Emit(OpCodes.Ldstr, "Method {0}.{1}.{2} is not service message.".FormatWith(ReflectionTools.GetNamespace(interfaceType), interfaceType.Name, method.Name));
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }));
                body.Emit(OpCodes.Throw);
                return;
            }

            var callOptionsCombine = GetCallOptionsCombine(message);
            if (callOptionsCombine == null)
            {
                // throw new NotSupportedException("Signature is not supported.");
                body.Emit(OpCodes.Ldstr, "Method {0}.{1}.{2} signature is not supported.".FormatWith(ReflectionTools.GetNamespace(interfaceType), interfaceType.Name, method.Name));
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }));
                body.Emit(OpCodes.Throw);
                return;
            }

            if (message.OperationType == MethodType.DuplexStreaming)
            {
                BuildDuplexStreamingMethod(message, grpcMethodFiled, callOptionsCombine, body);
                return;
            }

            body.DeclareLocal(callOptionsCombine.ReturnType); // var options
            body.DeclareLocal(message.RequestType); // var message

            // options = CallOptionsBuilder.Combine(context);
            foreach (var i in message.ContextInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Call, callOptionsCombine);
            body.Emit(OpCodes.Stloc_0);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_1);
            
            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(ClientBase).InstanceProperty("CallInvoker").GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            if (message.OperationType != MethodType.ClientStreaming)
            {
                body.Emit(OpCodes.Ldloc_1); // message
            }

            if (message.OperationType == MethodType.ServerStreaming)
            {
                // CallInvoker.AsyncServerStreamingCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncServerStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

                // GetServerStreamingCallResult(call, options)
                body.Emit(OpCodes.Ldloc_0); // options
                body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetServerStreamingCallResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));
            }
            else if (message.OperationType == MethodType.ClientStreaming)
            {
                // CallInvoker.AsyncClientStreamingCall()
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncClientStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

                // WriteClientStreamingRequest(call, request, options)
                body.EmitLdarg(message.RequestTypeInput[0] + 1);
                body.Emit(OpCodes.Ldloc_0); // options
                if (message.ResponseType.IsGenericType)
                {
                    body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.WriteClientStreamingRequest)).MakeGenericMethod(message.RequestType.GenericTypeArguments[0], message.ResponseType.GenericTypeArguments[0]));
                }
                else
                {
                    body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.WriteClientStreamingRequestWait)).MakeGenericMethod(message.RequestType.GenericTypeArguments[0]));
                }
            }
            else if (message.IsAsync)
            {
                // CallInvoker.AsyncUnaryCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncUnaryCall)).MakeGenericMethod(message.RequestType, message.ResponseType));
                if (message.ResponseType.IsGenericType)
                {
                    body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetAsyncUnaryCallResult)).MakeGenericMethod(message.ResponseType));
                }
                else
                {
                    body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.AsyncUnaryCallWait)));
                }
            }
            else
            {
                // CallInvoker.BlockingUnaryCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.BlockingUnaryCall)).MakeGenericMethod(message.RequestType, message.ResponseType));
                if (message.ResponseType.IsGenericType)
                {
                    // result.Value1
                    body.Emit(OpCodes.Callvirt, message.ResponseType.InstanceProperty("Value1").GetMethod);
                }
                else
                {
                    body.Emit(OpCodes.Pop);
                }
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreamingMethod(MessageAssembler message, FieldBuilder grpcMethodFiled, MethodInfo callOptionsCombine, ILGenerator body)
        {
            body.DeclareLocal(callOptionsCombine.ReturnType); // var options

            // options = CallOptionsBuilder.Combine(context);
            foreach (var i in message.ContextInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Call, callOptionsCombine);
            body.Emit(OpCodes.Stloc_0);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(ClientBase).InstanceProperty("CallInvoker").GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            // CallInvoker.AsyncDuplexStreamingCall
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncDuplexStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

            body.EmitLdarg(1); // request
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetDuplexCallResult)).MakeGenericMethod(message.RequestType.GenericTypeArguments[0], message.ResponseType.GenericTypeArguments[0]));

            body.Emit(OpCodes.Ret);
        }

        private void BuildCtor()
        {
            var parameterTypes = new[] { typeof(CallInvoker) };

            var ctor = _typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                parameterTypes);

            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(ClientBase).GetConstructor(parameterTypes));
            il.Emit(OpCodes.Ret);
        }
    }
}
