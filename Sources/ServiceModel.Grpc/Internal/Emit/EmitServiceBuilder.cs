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
    internal sealed class EmitServiceBuilder
    {
        private readonly TypeBuilder _typeBuilder;
        private readonly ILGenerator _ctor;
        private readonly Type _contractType;

        public EmitServiceBuilder(ModuleBuilder moduleBuilder, string className, Type contractType)
        {
            _contractType = contractType;

            _typeBuilder = moduleBuilder.DefineType(
                className,
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            _ctor = _typeBuilder
                .DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.HasThis,
                    new[] { contractType })
                .GetILGenerator();
            _ctor.Emit(OpCodes.Ldarg_0);
            _ctor.Emit(OpCodes.Call, typeof(object).Constructor());
        }

        public static bool IsSupportedContextInput(MessageAssembler message)
        {
            for (var i = 0; i < message.ContextInput.Length; i++)
            {
                var input = message.ContextInput[i];
                if (!ServerChannelAdapter.TryGetServiceContextOptionMethod(message.Parameters[input].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        public static Func<object, object> CreateFactory(Type implementationType, Type contractType)
        {
            var contract = Expression.Parameter(typeof(object), "contract");

            var ctor = implementationType.Constructor(contractType);
            var factory = Expression.New(
                ctor,
                Expression.Convert(contract, contractType));

            return Expression.Lambda<Func<object, object>>(factory, contract).Compile();
        }

        public void BuildNotSupportedOperation(OperationDescription operation, Type serviceType, string error)
        {
            var body = CreateMethodWithSignature(operation.Message, serviceType, operation.OperationName);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        public void BuildOperation(OperationDescription operation, Type serviceType)
        {
            var body = CreateMethodWithSignature(operation.Message, serviceType, operation.OperationName);
            var headersMarshallerFiled = InitializeHeadersMarshaller(operation);

            switch (operation.Message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, operation.Message, serviceType);
                    break;

                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, operation.Message, serviceType, headersMarshallerFiled);
                    break;

                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, operation.Message, serviceType);
                    break;

                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, operation.Message, serviceType, headersMarshallerFiled);
                    break;
            }
        }

        public Type BuildType()
        {
            _ctor.Emit(OpCodes.Ret);

            return _typeBuilder.CreateTypeInfo()!;
        }

        private void BuildUnary(ILGenerator body, MessageAssembler message, Type serviceType)
        {
            // service
            body.Emit(OpCodes.Ldarg_1);

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
                    body.Emit(OpCodes.Ldarg_2);
                    body.Emit(OpCodes.Callvirt, message.RequestType.InstanceProperty(propertyName).GetMethod);
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

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

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message, Type serviceType, FieldBuilder? headersMarshallerFiled)
        {
            DeclareHeaderValues(body, message, headersMarshallerFiled, 3);

            // service
            body.Emit(OpCodes.Ldarg_1);

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
                    body.Emit(OpCodes.Ldarg_2); // stream
                    body.Emit(OpCodes.Ldarg_3); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

            AdaptSyncUnaryCallResult(body, message);

            body.Emit(OpCodes.Ret);
        }

        private void BuildServerStreaming(ILGenerator body, MessageAssembler message, Type serviceType)
        {
            // service
            body.Emit(OpCodes.Ldarg_1);

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    PushContext(body, 4, parameter.ParameterType);
                }
                else
                {
                    var propertyName = "Value" + (Array.IndexOf(message.RequestTypeInput, i) + 1);

                    // request.Value1
                    body.Emit(OpCodes.Ldarg_2);
                    body.Emit(OpCodes.Callvirt, message.RequestType.InstanceProperty(propertyName).GetMethod);
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

            // ServerChannelAdapter.WriteServerStreamingResult(result, stream, serverCallContext);
            body.Emit(OpCodes.Ldarg_3); // stream
            body.EmitLdarg(4); // serverCallContext

            string adapterName;
            if (message.IsAsync)
            {
                adapterName = message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.WriteServerStreamingResultValueTask) : nameof(ServerChannelAdapter.WriteServerStreamingResultTask);
            }
            else
            {
                adapterName = nameof(ServerChannelAdapter.WriteServerStreamingResult);
            }

            if (message.ResponseType.IsGenericType)
            {
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(adapterName).MakeGenericMethod(message.ResponseType.GenericTypeArguments));
            }
            else
            {
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(adapterName).MakeGenericMethod(message.ResponseType));
            }

            body.Emit(OpCodes.Ret);
        }

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message, Type serviceType, FieldBuilder? headersMarshallerFiled)
        {
            DeclareHeaderValues(body, message, headersMarshallerFiled, 4);

            body.Emit(OpCodes.Ldarg_1); // service

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    PushContext(body, 4, parameter.ParameterType);
                }
                else if (message.HeaderRequestTypeInput.Contains(i))
                {
                    PushHeaderProperty(body, message, i);
                }
                else
                {
                    // ReadClientStream()
                    body.Emit(OpCodes.Ldarg_2); // input
                    body.EmitLdarg(4); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

            // ServerChannelAdapter.WriteServerStreamingResult
            body.Emit(OpCodes.Ldarg_3); // output
            body.EmitLdarg(4); // context

            string adapterName;
            if (message.IsAsync)
            {
                adapterName = message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.WriteServerStreamingResultValueTask) : nameof(ServerChannelAdapter.WriteServerStreamingResultTask);
            }
            else
            {
                adapterName = nameof(ServerChannelAdapter.WriteServerStreamingResult);
            }

            body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(adapterName).MakeGenericMethod(message.ResponseType.GenericTypeArguments));

            body.Emit(OpCodes.Ret);
        }

        private ILGenerator CreateMethodWithSignature(MessageAssembler message, Type serviceType, string methodName)
        {
            switch (message.OperationType)
            {
                case MethodType.Unary:
                    // Task<TResponse> Invoke(TService service, TRequest request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { serviceType, message.RequestType, typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ClientStreaming:
                    // Task<TResponse> Invoke(TService service, IAsyncStreamReader<TRequest> request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[] { serviceType, typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.ServerStreaming:
                    // Task Invoke(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task),
                            new[] { serviceType, message.RequestType, typeof(IServerStreamWriter<>).MakeGenericType(message.ResponseType), typeof(ServerCallContext) })
                        .GetILGenerator();

                case MethodType.DuplexStreaming:
                    // Task Invoke(TService service, IAsyncStreamReader<TRequest> request, IServerStreamWriter<TResponse> response, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[]
                            {
                                serviceType,
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
            body.Emit(OpCodes.Call, ServerChannelAdapter.GetServiceContextOptionMethod(contextType));
        }

        private void PushHeaderProperty(ILGenerator body, MessageAssembler message, int parameterIndex)
        {
            var propertyName = "Value" + (Array.IndexOf(message.HeaderRequestTypeInput, parameterIndex) + 1);
            body.Emit(OpCodes.Ldloc_0); // headers
            body.Emit(OpCodes.Callvirt, message.HeaderRequestType!.InstanceProperty(propertyName).GetMethod); // headers.Value1
        }

        private void DeclareHeaderValues(ILGenerator body, MessageAssembler message, FieldBuilder? headersMarshallerFiled, int contextParameterIndex)
        {
            if (headersMarshallerFiled != null)
            {
                body.DeclareLocal(message.HeaderRequestType!); // var headers

                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, headersMarshallerFiled); // static Marshaller<>
                body.EmitLdarg(contextParameterIndex); // context
                body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.GetMethodInputHeader)).MakeGenericMethod(message.HeaderRequestType));
                body.Emit(OpCodes.Stloc_0);
            }
        }

        private void CallContractMethod(ILGenerator body, MessageAssembler message, Type serviceType)
        {
            var parameters = new Type[message.Parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = message.Parameters[i].ParameterType;
            }

            body.Emit(OpCodes.Callvirt, serviceType.InstanceMethod(message.Operation.Name, parameters));
        }

        private FieldBuilder? InitializeHeadersMarshaller(OperationDescription operation)
        {
            if (operation.Message.HeaderRequestType == null)
            {
                return null;
            }

            var contractField = _contractType.InstanceFiled(operation.GrpcMethodHeaderName);

            // private Marshaller<Message<string, string>> _MethodMarshaller;
            var field = _typeBuilder
                .DefineField(
                    "_{0}".FormatWith(operation.GrpcMethodHeaderName),
                    contractField.FieldType,
                    FieldAttributes.Private | FieldAttributes.InitOnly);

            // _MethodMarshaller = contract.MethodMarshaller
            _ctor.Emit(OpCodes.Ldarg_0);
            _ctor.Emit(OpCodes.Ldarg_1);
            _ctor.Emit(OpCodes.Ldfld, contractField);
            _ctor.Emit(OpCodes.Stfld, field);

            return field;
        }
    }
}
