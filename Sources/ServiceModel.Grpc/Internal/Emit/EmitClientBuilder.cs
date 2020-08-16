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

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitClientBuilder
    {
        private readonly ContractDescription _description;
        private readonly Type _contractType;

        private TypeBuilder _typeBuilder = null!;
        private FieldBuilder _contractField = null!;
        private FieldBuilder _callInvokerField = null!;
        private FieldBuilder _callOptionsFactoryField = null!;

        public EmitClientBuilder(ContractDescription description, Type contractType)
        {
            _description = description;
            _contractType = contractType;
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
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder
            body.DeclareLocal(operation.Message.RequestType); // var message

            InitializeCallOptionsVariable(body, operation);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in operation.Message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, operation.Message.RequestType.Constructor(operation.Message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_2);

            // var invoker = __callInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callInvokerField);

            PushContractMethod(body, operation.GrpcMethodName);
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_2); // message

            if (operation.Message.IsAsync)
            {
                // CallInvoker.AsyncUnaryCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncUnaryCall)).MakeGenericMethod(operation.Message.RequestType, operation.Message.ResponseType));

                PushCallContext(body, operation.Message);

                if (operation.Message.ResponseType.IsGenericType)
                {
                    var adapter = typeof(ClientChannelAdapter)
                        .StaticMethod(nameof(ClientChannelAdapter.GetAsyncUnaryCallResult))
                        .MakeGenericMethod(operation.Message.ResponseType.GenericTypeArguments[0]);
                    body.Emit(OpCodes.Call, adapter);

                    // Task<> => new ValueTask<>
                    if (operation.Message.Operation.ReturnType.IsValueTask())
                    {
                        body.Emit(OpCodes.Newobj, typeof(ValueTask<>).MakeGenericType(operation.Message.ResponseType.GenericTypeArguments[0]).Constructor(adapter.ReturnType));
                    }
                }
                else
                {
                    var adapter = typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.AsyncUnaryCallWait));
                    body.Emit(OpCodes.Call, adapter);

                    // Task => new ValueTask
                    if (operation.Message.Operation.ReturnType.IsValueTask())
                    {
                        body.Emit(OpCodes.Newobj, typeof(ValueTask).Constructor(adapter.ReturnType));
                    }
                }
            }
            else
            {
                // CallInvoker.BlockingUnaryCall(...Method, null, context, value);
                body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.BlockingUnaryCall)).MakeGenericMethod(operation.Message.RequestType, operation.Message.ResponseType));
                if (operation.Message.ResponseType.IsGenericType)
                {
                    // result.Value1
                    body.Emit(OpCodes.Callvirt, operation.Message.ResponseType.InstanceProperty("Value1").GetMethod);
                }
                else
                {
                    body.Emit(OpCodes.Pop);
                }
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildServerStreaming(ILGenerator body, OperationDescription operation)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder
            body.DeclareLocal(operation.Message.RequestType); // var message

            InitializeCallOptionsVariable(body, operation);

            // message = new Message<string, string>(value12, value3);
            foreach (var i in operation.Message.RequestTypeInput)
            {
                body.EmitLdarg(i + 1);
            }

            body.Emit(OpCodes.Newobj, operation.Message.RequestType.Constructor(operation.Message.RequestType.GenericTypeArguments));
            body.Emit(OpCodes.Stloc_2);

            // var invoker = __callInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callInvokerField);

            PushContractMethod(body, operation.GrpcMethodName);
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options
            body.Emit(OpCodes.Ldloc_2); // message

            // CallInvoker.AsyncServerStreamingCall(...Method, null, context, value);
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncServerStreamingCall)).MakeGenericMethod(operation.Message.RequestType, operation.Message.ResponseType));

            // GetServerStreamingCallResult(call, options)
            PushCallContext(body, operation.Message);
            PushToken(body);
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetServerStreamingCallResult)).MakeGenericMethod(operation.Message.ResponseType.GenericTypeArguments[0]));

            if (operation.Message.IsAsync)
            {
                // IAsyncEnumerable<T>
                var adapterReturnType = operation.Message.Operation.ReturnType.GetGenericArguments()[0];

                if (operation.Message.Operation.ReturnType.IsValueTask())
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

        private void BuildClientStreaming(ILGenerator body, OperationDescription operation)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder

            InitializeCallOptionsVariable(body, operation);

            // var invoker = __callInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callInvokerField);

            PushContractMethod(body, operation.GrpcMethodName);
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            // CallInvoker.AsyncClientStreamingCall()
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncClientStreamingCall)).MakeGenericMethod(operation.Message.RequestType, operation.Message.ResponseType));

            // WriteClientStreamingRequest(call, request, context, token)
            body.EmitLdarg(operation.Message.RequestTypeInput[0] + 1);
            PushCallContext(body, operation.Message);
            PushToken(body);
            if (operation.Message.ResponseType.IsGenericType)
            {
                var adapter = typeof(ClientChannelAdapter)
                    .StaticMethod(nameof(ClientChannelAdapter.WriteClientStreamingRequest))
                    .MakeGenericMethod(operation.Message.RequestType.GenericTypeArguments[0], operation.Message.ResponseType.GenericTypeArguments[0]);
                body.Emit(OpCodes.Call, adapter);
            }
            else
            {
                var adapter = typeof(ClientChannelAdapter)
                    .StaticMethod(nameof(ClientChannelAdapter.WriteClientStreamingRequestWait))
                    .MakeGenericMethod(operation.Message.RequestType.GenericTypeArguments[0]);
                body.Emit(OpCodes.Call, adapter);
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreaming(ILGenerator body, OperationDescription operation)
        {
            body.DeclareLocal(typeof(CallOptions)); // var options
            body.DeclareLocal(typeof(CallOptionsBuilder)); // var optionsBuilder

            InitializeCallOptionsVariable(body, operation);

            // var invoker = __callInvoker
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callInvokerField);

            PushContractMethod(body, operation.GrpcMethodName);
            body.Emit(OpCodes.Ldnull); // var host = null
            body.Emit(OpCodes.Ldloc_0); // options

            // CallInvoker.AsyncDuplexStreamingCall
            body.Emit(OpCodes.Callvirt, typeof(CallInvoker).InstanceMethod(nameof(CallInvoker.AsyncDuplexStreamingCall)).MakeGenericMethod(operation.Message.RequestType, operation.Message.ResponseType));

            body.EmitLdarg(1); // request
            PushCallContext(body, operation.Message);
            PushToken(body);
            body.Emit(OpCodes.Call, typeof(ClientChannelAdapter).StaticMethod(nameof(ClientChannelAdapter.GetDuplexCallResult)).MakeGenericMethod(operation.Message.RequestType.GenericTypeArguments[0], operation.Message.ResponseType.GenericTypeArguments[0]));

            if (operation.Message.IsAsync)
            {
                // IAsyncEnumerable<T>
                var adapterReturnType = operation.Message.Operation.ReturnType.GetGenericArguments()[0];

                if (operation.Message.Operation.ReturnType.IsValueTask())
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

        private void InitializeCallOptionsVariable(ILGenerator body, OperationDescription operation)
        {
            // optionsBuilder = new CallOptionsBuilder(DefaultOptions)
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _callOptionsFactoryField); // DefaultOptions
            body.Emit(OpCodes.Newobj, typeof(CallOptionsBuilder).Constructor(typeof(Func<CallOptions>)));
            body.Emit(OpCodes.Stloc_1);

            // optionsBuilder = optionsBuilder.With()
            foreach (var i in operation.Message.ContextInput)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // optionsBuilder
                body.EmitLdarg(i + 1); // parameter

                var withMethodName = "With" + operation.Message.Parameters[i].ParameterType.Name;
                body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(withMethodName)); // .With
                body.Emit(OpCodes.Stloc_1);
            }

            if (operation.Message.HeaderRequestType != null)
            {
                body.Emit(OpCodes.Ldloca_S, 1); // optionsBuilder
                PushContractMethod(body, operation.GrpcMethodHeaderName); // Marshaller<>
                foreach (var i in operation.Message.HeaderRequestTypeInput)
                {
                    body.EmitLdarg(i + 1); // parameter
                }

                body.Emit(OpCodes.Newobj, operation.Message.HeaderRequestType.Constructor(operation.Message.HeaderRequestType.GenericTypeArguments)); // new Message<>
                body.Emit(OpCodes.Call, typeof(CallOptionsBuilder).InstanceMethod(nameof(CallOptionsBuilder.WithMethodInputHeader)).MakeGenericMethod(operation.Message.HeaderRequestType)); // .With
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
            var contextParameterIndex = -1;
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

        private void PushContractMethod(ILGenerator body, string methodFieldName)
        {
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, _contractField);
            body.Emit(OpCodes.Ldfld, _contractType.InstanceFiled(methodFieldName));
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
    }
}
