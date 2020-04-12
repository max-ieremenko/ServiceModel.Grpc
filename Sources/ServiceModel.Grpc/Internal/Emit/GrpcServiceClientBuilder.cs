using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class GrpcServiceClientBuilder : IServiceClientBuilder
    {
        private TypeBuilder _typeBuilder;
        private FieldBuilder _defaultCallOptions;
        private ILGenerator _defineGrpcMethod;

        public IMarshallerFactory MarshallerFactory { get; set; }

        public CallOptions? DefaultCallOptions { get; set; }

        public ILogger Logger { get; set; }

        public Func<CallInvoker, TContract> Build<TContract>(string factoryId)
        {
            Type implementationType;

            lock (ProxyAssembly.SyncRoot)
            {
                BuildCore(typeof(TContract), factoryId);
                implementationType = _typeBuilder.CreateTypeInfo();
            }

            return CreateFactory<TContract>(implementationType);
        }

        private void BuildCore(Type contractType, string factoryId)
        {
            _typeBuilder = ProxyAssembly
                .DefaultModule
                .DefineType(
                    "{0}.{1}Client{2}".FormatWith(ReflectionTools.GetNamespace(contractType), contractType.Name, factoryId),
                    TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                    typeof(GrpcClientBase));

            // private static CallOptions? DefaultCallOptions
            _defaultCallOptions = _typeBuilder
                .DefineField(
                    nameof(DefaultCallOptions),
                    typeof(CallOptions?),
                    FieldAttributes.Private | FieldAttributes.Static);

            _defineGrpcMethod = _typeBuilder
                .DefineMethod(
                    "DefineGrpcMethods",
                    MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public,
                    typeof(void),
                    new[] { typeof(IMarshallerFactory) })
                .GetILGenerator();

            // ctor(CallInvoker callInvoker)
            BuildCtor();

            foreach (var interfaceType in ContractDescription.GetInterfacesImplementation(contractType))
            {
                _typeBuilder.AddInterfaceImplementation(interfaceType);

                foreach (var method in ContractDescription.GetMethodsForImplementation(interfaceType))
                {
                    ImplementMethod(interfaceType, method);
                }
            }

            _defineGrpcMethod.Emit(OpCodes.Ret);
        }

        private Func<CallInvoker, TContract> CreateFactory<TContract>(Type implementationType)
        {
            var defineGrpcMethod = (Action<IMarshallerFactory>)implementationType
                .StaticMethod("DefineGrpcMethods")
                .CreateDelegate(typeof(Action<IMarshallerFactory>));
            defineGrpcMethod(MarshallerFactory);

            implementationType.StaticFiled(_defaultCallOptions.Name).SetValue(null, DefaultCallOptions);

            var callInvoker = Expression.Parameter(typeof(CallInvoker), "callInvoker");

            var ctor = Expression.New(implementationType.Constructor(typeof(CallInvoker)), callInvoker);

            return Expression.Lambda<Func<CallInvoker, TContract>>(ctor, callInvoker).Compile();
        }

        private FieldBuilder InitializeGrpcMethod(Type interfaceType, MessageAssembler message)
        {
            var filedType = typeof(Method<,>).MakeGenericType(message.RequestType, message.ResponseType);

            // private static Method<string, string> ConcatBMethod;
            var field = _typeBuilder
                .DefineField(
                    "{0}-{1}".FormatWith(interfaceType.Name, message.Operation.Name),
                    filedType,
                    FieldAttributes.Private | FieldAttributes.Static);

            var createMarshaller = typeof(IMarshallerFactory).InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller));

            _defineGrpcMethod.EmitLdcI4((int)message.OperationType); // MethodType
            _defineGrpcMethod.Emit(OpCodes.Ldstr, ServiceContract.GetServiceName(interfaceType));
            _defineGrpcMethod.Emit(OpCodes.Ldstr, ServiceContract.GetServiceOperationName(message.Operation));
            _defineGrpcMethod.Emit(OpCodes.Ldarg_0);
            _defineGrpcMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.RequestType));
            _defineGrpcMethod.Emit(OpCodes.Ldarg_0);
            _defineGrpcMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.ResponseType));
            _defineGrpcMethod.Emit(
                OpCodes.Newobj,
                filedType.Constructor(
                    typeof(MethodType),
                    typeof(string),
                    typeof(string),
                    typeof(Marshaller<>).MakeGenericType(message.RequestType),
                    typeof(Marshaller<>).MakeGenericType(message.ResponseType)));

            _defineGrpcMethod.Emit(OpCodes.Stsfld, field);

            return field;
        }

        private void ImplementMethod(Type interfaceType, MethodInfo method)
        {
            var body = CreateMethodWithSignature(method);

            if (!ContractDescription.IsOperationMethod(interfaceType, method))
            {
                var text = "Method {0}.{1}.{2} is not service operation.".FormatWith(
                    ReflectionTools.GetNamespace(interfaceType),
                    interfaceType.Name,
                    method.Name);

                // throw new NotSupportedException("...");
                body.Emit(OpCodes.Ldstr, text);
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
                body.Emit(OpCodes.Throw);

                Logger?.LogError(text);
                return;
            }

            if (!ContractDescription.TryCreateMessage(method, out var message, out var error))
            {
                // throw new NotSupportedException("...");
                body.Emit(OpCodes.Ldstr, error);
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
                body.Emit(OpCodes.Throw);

                Logger?.LogError(error);
                return;
            }

            var callOptionsCombine = ContractDescription.GetClientCallOptionsCombine(message);
            if (callOptionsCombine == null)
            {
                var text = "Context options in [{0}] are not supported.".FormatWith(ReflectionTools.GetSignature(method));

                // throw new NotSupportedException("...");
                body.Emit(OpCodes.Ldstr, text);
                body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
                body.Emit(OpCodes.Throw);

                Logger?.LogError(text);
                return;
            }

            var grpcMethodFiled = InitializeGrpcMethod(interfaceType, message);
            switch (message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, message, grpcMethodFiled, callOptionsCombine);
                    break;
                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, message, grpcMethodFiled, callOptionsCombine);
                    break;
                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, message, grpcMethodFiled, callOptionsCombine);
                    break;
                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, message, grpcMethodFiled, callOptionsCombine);
                    break;
                default:
                    throw new NotImplementedException("{0} operation is not implemented.".FormatWith(message.OperationType));
            }
        }

        private void BuildUnary(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, MethodInfo callOptionsCombine)
        {
            body.DeclareLocal(callOptionsCombine.ReturnType); // var options
            body.DeclareLocal(message.RequestType); // var message

            InitializeCallOptionsVariable(body, message, callOptionsCombine);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_1);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_1); // message
            
            if (message.IsAsync)
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

        private void BuildServerStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, MethodInfo callOptionsCombine)
        {
            body.DeclareLocal(callOptionsCombine.ReturnType); // var options
            body.DeclareLocal(message.RequestType); // var message

            InitializeCallOptionsVariable(body, message, callOptionsCombine);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_1);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_1); // message

            // CallInvoker.AsyncServerStreamingCall(...Method, null, context, value);
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncServerStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

            // GetServerStreamingCallResult(call, options)
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetServerStreamingCallResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));

            body.Emit(OpCodes.Ret);
        }

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, MethodInfo callOptionsCombine)
        {
            body.DeclareLocal(callOptionsCombine.ReturnType); // var options
            body.DeclareLocal(message.RequestType); // var message

            InitializeCallOptionsVariable(body, message, callOptionsCombine);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_1);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

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

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, MethodInfo callOptionsCombine)
        {
            body.DeclareLocal(callOptionsCombine.ReturnType); // var options

            InitializeCallOptionsVariable(body, message, callOptionsCombine);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

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

        private void InitializeCallOptionsVariable(ILGenerator body, MessageAssembler message, MethodInfo callOptionsCombine)
        {
            // options = CallOptionsBuilder.Combine(context);
            body.Emit(OpCodes.Ldsfld, _defaultCallOptions); // DefaultOptions
            foreach (var i in message.ContextInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Call, callOptionsCombine);
            body.Emit(OpCodes.Stloc_0);
        }

        private ILGenerator CreateMethodWithSignature(MethodInfo signature)
        {
            var parameterTypes = signature.GetParameters().Select(i => i.ParameterType).ToArray();
            var method = _typeBuilder
                .DefineMethod(
                    signature.Name,
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                    signature.ReturnType,
                    parameterTypes);

            // explicit interface implementation
            _typeBuilder.DefineMethodOverride(method, signature);

            return method.GetILGenerator();
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
            il.Emit(OpCodes.Call, typeof(GrpcClientBase).Constructor(parameterTypes));
            il.Emit(OpCodes.Ret);
        }
    }
}
