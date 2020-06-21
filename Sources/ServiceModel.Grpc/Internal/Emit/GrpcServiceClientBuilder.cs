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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class GrpcServiceClientBuilder : IServiceClientBuilder
    {
        private TypeBuilder _typeBuilder = null!;
        private FieldBuilder _defaultCallOptions = null!;
        private ILGenerator _defineGrpcMethod = null!;

        public IMarshallerFactory MarshallerFactory { get; set; } = null!;

        public Func<CallOptions>? DefaultCallOptionsFactory { get; set; }

        public ILogger? Logger { get; set; }

        public Func<CallInvoker, TContract> Build<TContract>(string factoryId)
        {
            Type implementationType;

            lock (ProxyAssembly.SyncRoot)
            {
                BuildCore(typeof(TContract), factoryId);
                implementationType = _typeBuilder.CreateTypeInfo()!;
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

            // private static CallOptions? DefaultCallOptionsFactory
            _defaultCallOptions = _typeBuilder
                .DefineField(
                    nameof(DefaultCallOptionsFactory),
                    typeof(Func<CallOptions>),
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

            var contractDescription = new ContractDescription(contractType);

            foreach (var interfaceDescription in contractDescription.Interfaces)
            {
                _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                    Logger?.LogDebug(method.Error);
                }
            }

            foreach (var interfaceDescription in contractDescription.Services)
            {
                _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                    Logger?.LogDebug(method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                    Logger?.LogError(method.Error);
                }

                foreach (var operation in interfaceDescription.Operations)
                {
                    ImplementMethod(interfaceDescription.InterfaceType, operation);
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

            implementationType.StaticFiled(_defaultCallOptions.Name).SetValue(null, DefaultCallOptionsFactory);

            var callInvoker = Expression.Parameter(typeof(CallInvoker), "callInvoker");

            var ctor = Expression.New(implementationType.Constructor(typeof(CallInvoker)), callInvoker);

            return Expression.Lambda<Func<CallInvoker, TContract>>(ctor, callInvoker).Compile();
        }

        private FieldBuilder? InitializeHeadersMarshaller(Type interfaceType, MessageAssembler message)
        {
            if (message.HeaderRequestType == null)
            {
                return null;
            }

            var filedType = typeof(Marshaller<>).MakeGenericType(message.HeaderRequestType);

            // private static Marshaller<Message<string, string>> ConcatBHeadersMarshaller;
            var field = _typeBuilder
                .DefineField(
                    "{0}-{1}-HeadersMarshaller".FormatWith(interfaceType.Name, message.Operation.Name),
                    filedType,
                    FieldAttributes.Private | FieldAttributes.Static);

            var createMarshaller = typeof(IMarshallerFactory).InstanceMethod(nameof(IMarshallerFactory.CreateMarshaller));

            _defineGrpcMethod.Emit(OpCodes.Ldarg_0);
            _defineGrpcMethod.Emit(OpCodes.Callvirt, createMarshaller.MakeGenericMethod(message.HeaderRequestType));
            _defineGrpcMethod.Emit(OpCodes.Stsfld, field);

            return field;
        }

        private FieldBuilder InitializeGrpcMethod(Type interfaceType, MessageAssembler message, string serviceName, string operationName)
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
            _defineGrpcMethod.Emit(OpCodes.Ldstr, serviceName);
            _defineGrpcMethod.Emit(OpCodes.Ldstr, operationName);
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

        private void ImplementNotSupportedMethod(MethodInfo method, string error)
        {
            var body = CreateMethodWithSignature(method);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        private void ImplementMethod(Type interfaceType, OperationDescription operation)
        {
            var message = operation.Message;
            var body = CreateMethodWithSignature(message.Operation);

            var grpcMethodFiled = InitializeGrpcMethod(interfaceType, message, operation.ServiceName, operation.OperationName);
            var headersMarshallerFiled = InitializeHeadersMarshaller(interfaceType, message);
            switch (message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, message, grpcMethodFiled);
                    break;
                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, message, grpcMethodFiled, headersMarshallerFiled);
                    break;
                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, message, grpcMethodFiled);
                    break;
                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, message, grpcMethodFiled, headersMarshallerFiled);
                    break;
                default:
                    throw new NotImplementedException("{0} operation is not implemented.".FormatWith(message.OperationType));
            }
        }

        private void BuildUnary(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder
            body.DeclareLocal(message.RequestType); // var message

            InitializeCallOptionsVariable(body, message, null);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_2);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_2); // message

            if (message.IsAsync)
            {
                // CallInvoker.AsyncUnaryCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncUnaryCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

                PushCallContext(body, message);

                if (message.ResponseType.IsGenericType)
                {
                    var adapter = typeof(ClientChannelAdapter)
                        .StaticMethod(nameof(ClientChannelAdapter.GetAsyncUnaryCallResult))
                        .MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]);
                    body.Emit(OpCodes.Call, adapter);

                    // Task<> => new ValueTask<>
                    if (message.Operation.ReturnType.IsValueTask())
                    {
                        body.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(message.ResponseType.GenericTypeArguments[0]).Constructor(adapter.ReturnType));
                    }
                }
                else
                {
                    var adapter = typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.AsyncUnaryCallWait));
                    body.Emit(OpCodes.Call, adapter);

                    // Task => new ValueTask
                    if (message.Operation.ReturnType.IsValueTask())
                    {
                        body.Emit(OpCodes.Newobj, typeof(ValueTask).Constructor(adapter.ReturnType));
                    }
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

        private void BuildServerStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder
            body.DeclareLocal(message.RequestType); // var message

            InitializeCallOptionsVariable(body, message, null);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, message.RequestType.Constructor(message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_2);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_2); // message

            // CallInvoker.AsyncServerStreamingCall(...Method, null, context, value);
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncServerStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

            // GetServerStreamingCallResult(call, options)
            PushCallContext(body, message);
            PushToken(body);
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetServerStreamingCallResult)).MakeGenericMethod(message.ResponseType.GenericTypeArguments[0]));

            if (message.IsAsync)
            {
                // IAsyncEnumerable<T>
                var adapterReturnType = message.Operation.ReturnType.GetGenericArguments()[0];

                if (message.Operation.ReturnType.IsValueTask())
                {
                    // new ValueTask(IAsyncEnumerable<T>)
                    body.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(adapterReturnType).Constructor(adapterReturnType));
                }
                else
                {
                    // Task.FromResult
                    body.Emit(OpCodes.Call, typeof(Task).StaticMethod(nameof(Task.FromResult)).MakeGenericMethod(adapterReturnType));
                }
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, FieldBuilder? headersMarshallerFiled)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder

            InitializeCallOptionsVariable(body, message, headersMarshallerFiled);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            // CallInvoker.AsyncClientStreamingCall()
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncClientStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

            // WriteClientStreamingRequest(call, request, context, token)
            body.EmitLdarg(message.RequestTypeInput[0] + 1);
            PushCallContext(body, message);
            PushToken(body);
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

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message, FieldBuilder grpcMethodFiled, FieldBuilder? headersMarshallerFiled)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder

            InitializeCallOptionsVariable(body, message, headersMarshallerFiled);

            // var invoker = base.CallInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Call, typeof(GrpcClientBase).InstanceProperty(nameof(GrpcClientBase.CallInvoker)).GetMethod);

            body.Emit(OpCodes.Ldsfld, grpcMethodFiled); // var method = static Method
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            // CallInvoker.AsyncDuplexStreamingCall
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncDuplexStreamingCall)).MakeGenericMethod(message.RequestType, message.ResponseType));

            body.EmitLdarg(1); // request
            PushCallContext(body, message);
            PushToken(body);
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetDuplexCallResult)).MakeGenericMethod(message.RequestType.GenericTypeArguments[0], message.ResponseType.GenericTypeArguments[0]));

            if (message.IsAsync)
            {
                // IAsyncEnumerable<T>
                var adapterReturnType = message.Operation.ReturnType.GetGenericArguments()[0];

                if (message.Operation.ReturnType.IsValueTask())
                {
                    // new ValueTask(IAsyncEnumerable<T>)
                    body.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(adapterReturnType).Constructor(adapterReturnType));
                }
                else
                {
                    // Task.FromResult
                    body.Emit(OpCodes.Call, typeof(Task).StaticMethod(nameof(Task.FromResult)).MakeGenericMethod(adapterReturnType));
                }
            }

            body.Emit(OpCodes.Ret);
        }

        private void InitializeCallOptionsVariable(ILGenerator body, MessageAssembler message, FieldBuilder? headersMarshallerFiled)
        {
            // optionsBuilder = new CallOptionsBuilder(DefaultOptions)
            body.Emit(OpCodes.Ldsfld, _defaultCallOptions); // DefaultOptions
            body.Emit(OpCodes.Newobj, typeof(CallOptionsBuilder).Constructor(typeof(Func<CallOptions>)));
            body.Emit(OpCodes.Stloc_1);

            // optionsBuilder = optionsBuilder.With()
            foreach (var i in message.ContextInput)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // optionsBuilder
                body.EmitLdarg(i + 1); // parameter

                var withMethodName = "With" + message.Parameters[i].ParameterType.Name;
                body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(withMethodName)); // .With
                body.Emit(OpCodes.Stloc_1);
            }

            if (headersMarshallerFiled != null)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // optionsBuilder
                body.Emit(OpCodes.Ldsfld, headersMarshallerFiled); // static Marshaller<>
                foreach (var i in message.HeaderRequestTypeInput)
                {
                    body.EmitLdarg(i + 1); // parameter
                }

                body.Emit(OpCodes.Newobj, message.HeaderRequestType!.Constructor(message.HeaderRequestType!.GenericTypeArguments)); // new Message<>
                body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(nameof(CallOptionsBuilder.WithMethodInputHeader)).MakeGenericMethod(message.HeaderRequestType)); // .With
                body.Emit(OpCodes.Stloc_1);
            }

            // options = optionsBuilder.Build()
            body.Emit(OpCodes.Ldloca_S, 1); // optionsBuilder
            body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(nameof(CallOptionsBuilder.Build)));
            body.Emit(OpCodes.Stloc_0);
        }

        private void PushToken(ILGenerator body)
        {
            body.Emit(OpCodes.Ldloca_S, 0); // options
            body.Emit(OpCodes.Call, typeof(CallOptions).InstanceProperty(nameof(CallOptions.CancellationToken)).GetMethod); // options.CancellationToken
        }

        private void PushCallContext(ILGenerator body, MessageAssembler message)
        {
            int contextParameterIndex = -1;
            foreach (var i in message.ContextInput)
            {
                if (message.Parameters[i].ParameterType == typeof(CallContext))
                {
                    contextParameterIndex = i;
                    break;
                }
            }

            if (contextParameterIndex < 0)
            {
                body.Emit(OpCodes.Ldnull); // context = null
            }
            else
            {
                body.EmitLdarg(contextParameterIndex + 1); // context parameter
            }
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

            if (signature.IsGenericMethod)
            {
                var genericParameters = signature.GetGenericArguments().Select(i => i.Name).ToArray();
                method.DefineGenericParameters(genericParameters);
            }

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
