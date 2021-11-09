// <copyright>
// Copyright 2020-2021 Max Ieremenko
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
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitServiceEndpointBuilder
    {
        private readonly ContractDescription _description;
        private readonly Type _contractType;
        private readonly HashSet<string> _uniqueMemberNames;

        private TypeBuilder _typeBuilder = null!;
        private ILGenerator _ctor = null!;

        public EmitServiceEndpointBuilder(ContractDescription description, Type contractType)
        {
            _description = description;
            _contractType = contractType;
            _uniqueMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

        public TypeInfo Build(ModuleBuilder moduleBuilder, ILogger? logger = default, string? className = default)
        {
            _typeBuilder = moduleBuilder.DefineType(
                className ?? _description.EndpointClassName,
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

            _ctor = _typeBuilder
                .DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.HasThis,
                    new[] { _contractType })
                .GetILGenerator();
            _ctor.Emit(OpCodes.Ldarg_0);
            _ctor.Emit(OpCodes.Call, typeof(object).Constructor());

            foreach (var interfaceDescription in _description.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    if (IsSupportedContextInput(operation.Message))
                    {
                        BuildOperation(operation, interfaceDescription.InterfaceType);
                    }
                    else
                    {
                        var error = "Context options in [{0}] are not supported.".FormatWith(ReflectionTools.GetSignature(operation.Message.Operation));
                        logger?.LogError("Service {0}: {1}", _description.ServiceType.FullName, error);
                        BuildNotSupportedOperation(operation, interfaceDescription.InterfaceType, error);
                    }
                }
            }

            _ctor.Emit(OpCodes.Ret);

            return _typeBuilder.CreateTypeInfo()!;
        }

        private void BuildNotSupportedOperation(OperationDescription operation, Type serviceType, string error)
        {
            var body = CreateMethodWithSignature(operation.Message, serviceType, operation.OperationName);

            // throw new NotSupportedException("...");
            body.Emit(OpCodes.Ldstr, error);
            body.Emit(OpCodes.Newobj, typeof(NotSupportedException).Constructor(typeof(string)));
            body.Emit(OpCodes.Throw);
        }

        private void BuildOperation(OperationDescription operation, Type serviceType)
        {
            var body = CreateMethodWithSignature(operation.Message, serviceType, operation.OperationName);
            var outputHeadersMarshaller = InitializeOutputHeadersMarshaller(operation);

            switch (operation.Message.OperationType)
            {
                case MethodType.Unary:
                    BuildUnary(body, operation.Message, serviceType);
                    break;

                case MethodType.ClientStreaming:
                    BuildClientStreaming(body, operation.Message, serviceType);
                    break;

                case MethodType.ServerStreaming:
                    BuildServerStreaming(body, operation.Message, serviceType, outputHeadersMarshaller);
                    break;

                case MethodType.DuplexStreaming:
                    BuildDuplexStreaming(body, operation.Message, serviceType, outputHeadersMarshaller);
                    break;
            }
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

        private void BuildClientStreaming(ILGenerator body, MessageAssembler message, Type serviceType)
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
                else if (message.HeaderRequestTypeInput.Contains(i))
                {
                    PushHeaderProperty(body, message, i);
                }
                else
                {
                    // ReadClientStream()
                    body.Emit(OpCodes.Ldarg_3); // stream
                    body.EmitLdarg(4); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

            AdaptSyncUnaryCallResult(body, message);

            body.Emit(OpCodes.Ret);
        }

        private void BuildServerStreaming(ILGenerator body, MessageAssembler message, Type serviceType, FieldBuilder? outputHeadersMarshaller)
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

            BuildWriteServerStreamingResult(body, message, outputHeadersMarshaller, 3, 4);

            body.Emit(OpCodes.Ret);
        }

        private (MethodInfo Method, Type DelegateType) BuildServerStreamingResultAdapter(MessageAssembler message)
        {
            var parameterType = message.Operation.ReturnType.GetGenericArguments()[0];
            var returnType = typeof(ValueTuple<,>).MakeGenericType(
                message.HeaderResponseType,
                typeof(IAsyncEnumerable<>).MakeGenericType(message.ResponseType.GenericTypeArguments[0]));

            var method = _typeBuilder
                .DefineMethod(
                    GetUniqueMemberName("Adapt" + message.Operation.Name + "Response"),
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                    returnType,
                    new[] { parameterType });

            var body = method.GetILGenerator();

            var headerPropertiesCount = message.HeaderResponseTypeInput.Length;
            for (var i = 0; i < headerPropertiesCount; i++)
            {
                body.Emit(OpCodes.Ldarg_0);

                var index = message.HeaderResponseTypeInput[i] + 1;
                var fieldName = "Item" + index.ToString(CultureInfo.InvariantCulture);

                body.Emit(OpCodes.Ldfld, parameterType.InstanceFiled(fieldName));
            }

            // new Header()
            body.Emit(OpCodes.Newobj, message.HeaderResponseType!.Constructor(headerPropertiesCount));

            // push stream
            var streamFieldName = "Item" + (message.ResponseTypeIndex + 1).ToString(CultureInfo.InvariantCulture);
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldfld, parameterType.InstanceFiled(streamFieldName));

            body.Emit(OpCodes.Newobj, returnType!.Constructor(2));
            body.Emit(OpCodes.Ret);

            var delegateType = typeof(Func<,>).MakeGenericType(parameterType, returnType);
            return (method, delegateType);
        }

        private void BuildDuplexStreaming(ILGenerator body, MessageAssembler message, Type serviceType, FieldBuilder? outputHeadersMarshaller)
        {
            body.Emit(OpCodes.Ldarg_1); // service

            for (var i = 0; i < message.Parameters.Length; i++)
            {
                var parameter = message.Parameters[i];
                if (message.ContextInput.Contains(i))
                {
                    PushContext(body, 5, parameter.ParameterType);
                }
                else if (message.HeaderRequestTypeInput.Contains(i))
                {
                    PushHeaderProperty(body, message, i);
                }
                else
                {
                    // ReadClientStream()
                    body.Emit(OpCodes.Ldarg_3); // request
                    body.EmitLdarg(5); // context
                    body.Emit(OpCodes.Call, typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.ReadClientStream)).MakeGenericMethod(message.RequestType.GenericTypeArguments));
                }
            }

            // service.Method
            CallContractMethod(body, message, serviceType);

            BuildWriteServerStreamingResult(body, message, outputHeadersMarshaller, 4, 5);

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
                    // Task<TResponse> Invoke(TService service, Message<TRequestHeader>? requestHeader, IAsyncStreamReader<TRequest> request, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task<>).MakeGenericType(message.ResponseType),
                            new[]
                            {
                                serviceType,
                                message.HeaderRequestType ?? typeof(Message),
                                typeof(IAsyncStreamReader<>).MakeGenericType(message.RequestType),
                                typeof(ServerCallContext)
                            })
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
                    // Task Invoke(TService service, Message<TRequestHeader>? requestHeader, IAsyncStreamReader<TRequest> request, IServerStreamWriter<TResponse> response, ServerCallContext context)
                    return _typeBuilder
                        .DefineMethod(
                            methodName,
                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                            typeof(Task),
                            new[]
                            {
                                serviceType,
                                message.HeaderRequestType ?? typeof(Message),
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
            body.Emit(OpCodes.Ldarg_2); // requestHeader
            body.Emit(OpCodes.Callvirt, message.HeaderRequestType!.InstanceProperty(propertyName).GetMethod); // requestHeader
        }

        private void CallContractMethod(ILGenerator body, MessageAssembler message, Type serviceType)
        {
            var parameters = new Type[message.Parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = message.Parameters[i].ParameterType;
            }

            var method = serviceType.InstanceMethod(message.Operation.Name, parameters);
            body.Emit(OpCodes.Callvirt, method);
        }

        private FieldBuilder? InitializeOutputHeadersMarshaller(OperationDescription operation)
        {
            if (operation.Message.HeaderResponseType == null)
            {
                return null;
            }

            var contractField = _contractType.InstanceFiled(operation.GrpcMethodOutputHeaderName);

            // private Marshaller<Message<string, string>> _MethodMarshaller;
            var field = _typeBuilder
                .DefineField(
                    "_{0}".FormatWith(operation.GrpcMethodOutputHeaderName),
                    contractField.FieldType,
                    FieldAttributes.Private | FieldAttributes.InitOnly);

            // _MethodMarshaller = contract.MethodMarshaller
            _ctor.Emit(OpCodes.Ldarg_0);
            _ctor.Emit(OpCodes.Ldarg_1);
            _ctor.Emit(OpCodes.Ldfld, contractField);
            _ctor.Emit(OpCodes.Stfld, field);

            return field;
        }

        private void BuildWriteServerStreamingResult(
            ILGenerator body,
            MessageAssembler message,
            FieldBuilder? outputHeadersMarshaller,
            int responseParameterIndex,
            int contextParameterIndex)
        {
            if (message.HeaderResponseType == null)
            {
                // ServerChannelAdapter.WriteServerStreamingResult(result, stream, serverCallContext);
                body.EmitLdarg(responseParameterIndex); // response
                body.EmitLdarg(contextParameterIndex); // serverCallContext

                MethodInfo channelAdapter;
                if (message.IsAsync)
                {
                    channelAdapter = typeof(ServerChannelAdapter)
                        .StaticMethod(message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.WriteServerStreamingResultValueTask) : nameof(ServerChannelAdapter.WriteServerStreamingResultTask));
                }
                else
                {
                    channelAdapter = typeof(ServerChannelAdapter).StaticMethod(nameof(ServerChannelAdapter.WriteServerStreamingResult), 3);
                }

                body.Emit(OpCodes.Call, channelAdapter.MakeGenericMethod(message.ResponseType.GenericTypeArguments));
            }
            else
            {
                var (adapter, adapterType) = BuildServerStreamingResultAdapter(message);

                // new Func<TResponse, (THeader Header, IAsyncEnumerable<TResult> Stream)> convertResponse
                body.Emit(OpCodes.Ldnull);
                body.Emit(OpCodes.Ldftn, adapter);
                body.Emit(OpCodes.Newobj, adapterType.Constructor(typeof(object), typeof(IntPtr)));

                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, outputHeadersMarshaller!); // Marshaller<>

                body.EmitLdarg(responseParameterIndex); // response
                body.EmitLdarg(contextParameterIndex); // serverCallContext

                var channelAdapter = typeof(ServerChannelAdapter)
                    .StaticMethod(message.Operation.ReturnType.IsValueTask() ? nameof(ServerChannelAdapter.WriteServerStreamingResultWithHeaderValueTask) : nameof(ServerChannelAdapter.WriteServerStreamingResultWithHeaderTask))
                    .MakeGenericMethod(message.Operation.ReturnType.GetGenericArguments()[0], message.HeaderResponseType, message.ResponseType.GenericTypeArguments[0]);
                body.Emit(OpCodes.Call, channelAdapter);
            }
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
