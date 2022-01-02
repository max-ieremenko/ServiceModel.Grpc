// <copyright>
// Copyright 2020-2022 Max Ieremenko
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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Client.Internal;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitClientBuilder
    {
        private readonly ContractDescription _description;
        private readonly Type _contractType;
        private readonly HashSet<string> _uniqueMemberNames;

        private TypeBuilder _typeBuilder = null!;
        private FieldBuilder _contractField = null!;
        private FieldBuilder _callInvokerField = null!;
        private FieldBuilder _callOptionsFactoryField = null!;

        public EmitClientBuilder(ContractDescription description, Type contractType)
        {
            _description = description;
            _contractType = contractType;
            _uniqueMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public TypeInfo Build(ModuleBuilder moduleBuilder)
        {
            _typeBuilder = moduleBuilder
                .DefineType(
                    _description.ClientClassName,
                    TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            _contractField = _typeBuilder
                .DefineField(
                    "__contract",
                    _contractType,
                    FieldAttributes.Private | FieldAttributes.InitOnly);

            _callInvokerField = _typeBuilder
                .DefineField(
                    "__callInvoker",
                    typeof(CallInvoker),
                    FieldAttributes.Private | FieldAttributes.InitOnly);

            _callOptionsFactoryField = _typeBuilder
                .DefineField(
                    "__callOptionsFactory",
                    typeof(Func<CallOptions>),
                    FieldAttributes.Private | FieldAttributes.InitOnly);

            BuildCtor();

            foreach (var interfaceDescription in _description.Interfaces)
            {
                _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                }
            }

            foreach (var interfaceDescription in _description.Services)
            {
                _typeBuilder.AddInterfaceImplementation(interfaceDescription.InterfaceType);

                foreach (var method in interfaceDescription.Methods)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                }

                foreach (var method in interfaceDescription.NotSupportedOperations)
                {
                    ImplementNotSupportedMethod(method.Method, method.Error);
                }

                foreach (var operation in interfaceDescription.Operations)
                {
                    ImplementMethod(operation);
                }
            }

            return _typeBuilder.CreateTypeInfo()!;
        }

        public Func<CallInvoker, object, Func<CallOptions>?, TContract> CreateFactory<TContract>(Type implementationType)
        {
            var callInvoker = Expression.Parameter(typeof(CallInvoker), "callInvoker");
            var contract = Expression.Parameter(typeof(object), "contract");
            var callOptions = Expression.Parameter(typeof(Func<CallOptions>), "callOptions");

            var ctor = implementationType.Constructor(typeof(CallInvoker), _contractType, typeof(Func<CallOptions>));
            var factory = Expression.New(
                ctor,
                callInvoker,
                Expression.Convert(contract, _contractType),
                callOptions);

            return Expression.Lambda<Func<CallInvoker, object, Func<CallOptions>?, TContract>>(factory, callInvoker, contract, callOptions).Compile();
        }

        private void ImplementNotSupportedMethod(MethodInfo method, string error)
        {
            var body = CreateMethodWithSignature(method);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        private void ImplementMethod(OperationDescription operation)
        {
            var message = operation.Message;
            var body = CreateMethodWithSignature(message.Operation);

            switch (message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, operation);
                    break;
                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, operation);
                    break;
                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, operation);
                    break;
                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, operation);
                    break;
                default:
                    throw new NotImplementedException("{0} operation is not implemented.".FormatWith(message.OperationType));
            }
        }

        private void BuildUnary(ILGenerator body, OperationDescription operation)
        {
            // optionsBuilder
            InitializeCallOptionsBuilder(body, operation);

            // var call = new UnaryCall<TRequest, TResponse>(method, CallInvoker, optionsBuilder)
            var callType = typeof(UnaryCall<,>)
                .MakeGenericType(operation.Message.RequestType, operation.Message.ResponseType);
            InitializeCall(body, operation, callType);

            // call.Invoke(message)
            body.Emit(OpCodes.Ldloca_S, 1); // call
            PushMessage(body, operation.Message.RequestType, operation.Message.RequestTypeInput); // message

            var invokeMethod = callType.InstanceGenericMethod(
                operation.Message.IsAsync ? "InvokeAsync" : "Invoke",
                operation.Message.ResponseType.IsGenericType ? 1 : 0);
            if (operation.Message.ResponseType.IsGenericType)
            {
                invokeMethod = invokeMethod.MakeGenericMethod(operation.Message.ResponseType.GenericTypeArguments);
            }

            body.Emit(OpCodes.Call, invokeMethod);

            // Task => new ValueTask
            if (operation.Message.Operation.ReturnType.IsValueTask())
            {
                body.Emit(OpCodes.Newobj, operation.Message.Operation.ReturnType.Constructor(invokeMethod.ReturnType));
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildServerStreaming(ILGenerator body, OperationDescription operation)
        {
            // optionsBuilder
            InitializeCallOptionsBuilder(body, operation);

            // var call = new ServerStreamingCall<TRequest, TResponseHeader, TResponse>(method, CallInvoker, optionsBuilder)
            var callType = typeof(ServerStreamingCall<,,>).MakeGenericType(
                    operation.Message.RequestType,
                    operation.Message.HeaderResponseType ?? typeof(Message),
                    operation.Message.ResponseType.GenericTypeArguments[0]);
            InitializeCall(body, operation, callType);

            // call.InvokeAsync(message)
            body.Emit(OpCodes.Ldloca_S, 1); // call
            PushMessage(body, operation.Message.RequestType, operation.Message.RequestTypeInput); // message

            DoServerStreamingCall(body, operation, callType);

            body.Emit(OpCodes.Ret);
        }

        private (MethodInfo Method, Type DelegateType) BuildServerStreamingResultAdapter(OperationDescription operation)
        {
            var parameterHeaderType = operation.Message.HeaderResponseType!;
            var parameterStreamType = typeof(IAsyncEnumerable<>).MakeGenericType(operation.Message.ResponseType.GetGenericArguments()[0]);

            var returnType = operation.Message.Operation.ReturnType.GetGenericArguments()[0];

            var method = _typeBuilder
                .DefineMethod(
                    GetUniqueMemberName("Adapt" + operation.Message.Operation.Name + "Response"),
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                    returnType,
                    new[] { parameterHeaderType, parameterStreamType });

            var body = method.GetILGenerator();

            var propertiesCount = operation.Message.HeaderResponseTypeInput.Length + 1;
            for (var i = 0; i < propertiesCount; i++)
            {
                if (i == operation.Message.ResponseTypeIndex)
                {
                    body.Emit(OpCodes.Ldarg_1);
                }
                else
                {
                    var index = Array.IndexOf(operation.Message.HeaderResponseTypeInput, i) + 1;
                    var propertyName = "Value" + index.ToString(CultureInfo.InvariantCulture);

                    body.Emit(OpCodes.Ldarg_0);
                    body.Emit(OpCodes.Callvirt, parameterHeaderType.InstanceProperty(propertyName).GetMethod);
                }
            }

            body.Emit(OpCodes.Newobj, returnType.Constructor(propertiesCount));
            body.Emit(OpCodes.Ret);

            var delegateType = typeof(Func<,,>).MakeGenericType(parameterHeaderType, parameterStreamType, returnType);
            return (method, delegateType);
        }

        private void BuildClientStreaming(ILGenerator body, OperationDescription operation)
        {
            // optionsBuilder
            InitializeCallOptionsBuilder(body, operation);

            // var call = new ClientStreamingCall<TRequestHeader, TRequest, TResponse>(method, CallInvoker, optionsBuilder)
            var callType = typeof(ClientStreamingCall<,,>).MakeGenericType(
                operation.Message.HeaderRequestType ?? typeof(Message),
                operation.Message.RequestType.GenericTypeArguments[0],
                operation.Message.ResponseType);
            InitializeCall(body, operation, callType);

            // call.InvokeAsync(stream)
            body.Emit(OpCodes.Ldloca_S, 1); // call
            body.EmitLdarg(operation.Message.RequestTypeInput[0] + 1);

            MethodInfo invokeMethod;
            if (operation.Message.ResponseType.IsGenericType)
            {
                invokeMethod = callType
                    .InstanceGenericMethod("InvokeAsync", 1)
                    .MakeGenericMethod(operation.Message.ResponseType.GenericTypeArguments[0]);
            }
            else
            {
                invokeMethod = callType.InstanceGenericMethod("InvokeAsync", 0);
            }

            body.Emit(OpCodes.Call, invokeMethod);

            if (operation.Message.Operation.ReturnType.IsValueTask())
            {
                // new ValueTask<>()
                body.Emit(OpCodes.Newobj, operation.Message.Operation.ReturnType.Constructor(invokeMethod.ReturnType));
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreaming(ILGenerator body, OperationDescription operation)
        {
            // optionsBuilder
            InitializeCallOptionsBuilder(body, operation);

            // var call = DuplexStreamingCall<TRequestHeader, TRequest, TResponseHeader, TResponse>(method, CallInvoker, optionsBuilder)
            var callType = typeof(DuplexStreamingCall<,,,>).MakeGenericType(
                operation.Message.HeaderRequestType ?? typeof(Message),
                operation.Message.RequestType.GenericTypeArguments[0],
                operation.Message.HeaderResponseType ?? typeof(Message),
                operation.Message.ResponseType.GenericTypeArguments[0]);
            InitializeCall(body, operation, callType);

            // call.InvokeAsync(stream)
            body.Emit(OpCodes.Ldloca_S, 1); // call
            body.EmitLdarg(operation.Message.RequestTypeInput[0] + 1);

            DoServerStreamingCall(body, operation, callType);

            body.Emit(OpCodes.Ret);
        }

        private void InitializeCallOptionsBuilder(ILGenerator body, OperationDescription operation)
        {
            // var optionsBuilder = new CallOptionsBuilder(DefaultOptions)
            body.DeclareLocal(typeof(CallOptionsBuilder));
            body.Emit(OpCodes.Ldarg_0); // this
            body.Emit(OpCodes.Ldfld, _callOptionsFactoryField); // DefaultOptions
            body.Emit(OpCodes.Newobj, typeof(CallOptionsBuilder).Constructor(typeof(Func<CallOptions>)));
            body.Emit(OpCodes.Stloc_0);

            // optionsBuilder = optionsBuilder.With()
            foreach (var i in operation.Message.ContextInput)
            {
                body.Emit(OpCodes.Ldloca_S, 0); // optionsBuilder
                body.EmitLdarg(i + 1); // parameter

                var withMethodName = "With" + operation.Message.Parameters[i].ParameterType.Name;
                body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(withMethodName)); // .With
                body.Emit(OpCodes.Stloc_0);
            }
        }

        private void InitializeCall(ILGenerator body, OperationDescription operation, Type callType)
        {
            // var call = new callType(...)
            body.DeclareLocal(callType); // var call
            PushContractField(body, operation.GrpcMethodName); // method

            body.Emit(OpCodes.Ldarg_0); // CallInvoker
            body.Emit(OpCodes.Ldfld, _callInvokerField);

            body.Emit(OpCodes.Ldloca_S, 0); // optionsBuilder
            body.Emit(OpCodes.Newobj, callType.Constructor(3));
            body.Emit(OpCodes.Stloc_1);

            if (operation.Message.HeaderResponseType != null)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // call

                PushContractField(body, operation.GrpcMethodOutputHeaderName);

                body.Emit(OpCodes.Call, callType.InstanceMethod("WithResponseHeader"));
                body.Emit(OpCodes.Stloc_1);
            }

            if (operation.Message.HeaderRequestType != null)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // call

                PushContractField(body, operation.GrpcMethodInputHeaderName);
                PushMessage(body, operation.Message.HeaderRequestType, operation.Message.HeaderRequestTypeInput);

                body.Emit(OpCodes.Call, callType.InstanceMethod("WithRequestHeader"));
                body.Emit(OpCodes.Stloc_1);
            }
        }

        private void DoServerStreamingCall(ILGenerator body, OperationDescription operation, Type callType)
        {
            if (operation.Message.HeaderResponseType == null && operation.Message.IsAsync)
            {
                var invokeMethod = callType.InstanceGenericMethod("InvokeAsync", 0);
                body.Emit(OpCodes.Call, invokeMethod);

                if (operation.Message.Operation.ReturnType.IsValueTask())
                {
                    // new ValueTask<>()
                    body.Emit(OpCodes.Newobj, operation.Message.Operation.ReturnType.Constructor(invokeMethod.ReturnType));
                }
            }
            else if (operation.Message.HeaderResponseType == null)
            {
                var invokeMethod = callType.InstanceMethod("Invoke");
                body.Emit(OpCodes.Call, invokeMethod);
            }
            else
            {
                var (adapter, adapterType) = BuildServerStreamingResultAdapter(operation);

                // new Func<,>(Adapter)
                body.Emit(OpCodes.Ldnull);
                body.Emit(OpCodes.Ldftn, adapter);
                body.Emit(OpCodes.Newobj, adapterType.Constructor(typeof(object), typeof(IntPtr)));

                var invokeMethod = callType
                    .InstanceGenericMethod("InvokeAsync", 1)
                    .MakeGenericMethod(operation.Message.Operation.ReturnType.GetGenericArguments()[0]);
                body.Emit(OpCodes.Call, invokeMethod);

                if (operation.Message.Operation.ReturnType.IsValueTask())
                {
                    // new ValueTask<>()
                    body.Emit(OpCodes.Newobj, operation.Message.Operation.ReturnType.Constructor(invokeMethod.ReturnType));
                }
            }
        }

        private void PushContractField(ILGenerator body, string fieldName)
        {
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _contractField);
            body.Emit(OpCodes.Ldfld, _contractType.InstanceFiled(fieldName));
        }

        private void PushMessage(ILGenerator body, Type messageType, int[] messageInput)
        {
            // new Message<string, string>(value12, value3);
            foreach (var i in messageInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, messageType.Constructor(messageType.GenericTypeArguments));
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
            var ctor = _typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                new[] { typeof(CallInvoker), _contractType, typeof(Func<CallOptions>) });

            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(object).Constructor());

            // __callInvoker
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _callInvokerField);

            // __contract
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, _contractField);

            // __callOptionsFactory
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Stfld, _callOptionsFactoryField);

            il.Emit(OpCodes.Ret);
        }

        private string GetUniqueMemberName(string suggestedName)
        {
            var index = 1;
            var result = suggestedName;

            while (!_uniqueMemberNames.Add(result))
            {
                result = suggestedName + index.ToString(CultureInfo.InvariantCulture);
                index++;
            }

            return result;
        }
    }
}
