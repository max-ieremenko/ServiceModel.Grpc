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
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal sealed class EmitServiceEndpointBinder<TService> : IServiceEndpointBinder<TService>
    {
        private readonly ContractDescription _description;
        private readonly Type? _serviceInstanceType;
        private readonly Type _contractType;
        private readonly Type _channelType;
        private readonly ILogger? _logger;

        private readonly MethodInfo _serviceBinderAddUnaryMethod;
        private readonly MethodInfo _serviceBinderAddClientStreamingMethod;
        private readonly MethodInfo _serviceBinderAddServerStreamingMethod;
        private readonly MethodInfo _serviceBinderAddDuplexStreamingMethod;

        public EmitServiceEndpointBinder(
            ContractDescription description,
            Type? serviceInstanceType,
            Type contractType,
            Type channelType,
            ILogger? logger)
        {
            _description = description;
            _serviceInstanceType = serviceInstanceType;
            _contractType = contractType;
            _channelType = channelType;
            _logger = logger;

            _serviceBinderAddUnaryMethod = GetType().StaticMethod(nameof(AddUnaryMethod));
            _serviceBinderAddClientStreamingMethod = GetType().StaticMethod(nameof(AddClientStreamingMethod));
            _serviceBinderAddServerStreamingMethod = GetType().StaticMethod(nameof(AddServerStreamingMethod));
            _serviceBinderAddDuplexStreamingMethod = GetType().StaticMethod(nameof(AddDuplexStreamingMethod));
        }

        public void Bind(IServiceMethodBinder<TService> binder)
        {
            var contract = EmitContractBuilder.CreateFactory(_contractType)(binder.MarshallerFactory);
            var channelInstance = EmitServiceEndpointBuilder.CreateFactory(_channelType, _contractType)(contract);
            var serviceType = typeof(TService);

            foreach (var interfaceDescription in _description.Services)
            {
                foreach (var operation in interfaceDescription.Operations)
                {
                    var message = operation.Message;
                    var channelMethod = _channelType.InstanceMethod(operation.OperationName);
                    var metadata = TryGetMethodMetadata(interfaceDescription.InterfaceType, message.Operation);
                    var grpcMethodMethod = (IMethod)_contractType.InstanceFiled(operation.GrpcMethodName).GetValue(contract);

                    object? requestHeaderMarshaller = null;
                    if (message.HeaderRequestType != null)
                    {
                        requestHeaderMarshaller = _contractType.InstanceFiled(operation.GrpcMethodInputHeaderName).GetValue(contract);
                    }

                    _logger?.LogDebug("Bind service method {0}.{1}.", serviceType.FullName, message.Operation.Name);
                    if (grpcMethodMethod.Type == MethodType.Unary)
                    {
                        var addMethod = _serviceBinderAddUnaryMethod
                            .MakeGenericMethod(message.RequestType, message.ResponseType)
                            .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, IList<object>, MethodInfo, object>>();
                        addMethod(binder, grpcMethodMethod, metadata, channelMethod, channelInstance);
                    }
                    else if (grpcMethodMethod.Type == MethodType.ClientStreaming)
                    {
                        var addMethod = _serviceBinderAddClientStreamingMethod
                            .MakeGenericMethod(message.HeaderRequestType ?? typeof(Message), message.RequestType.GenericTypeArguments[0], message.ResponseType)
                            .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, object?, IList<object>, MethodInfo, object>>();
                        addMethod(binder, grpcMethodMethod, requestHeaderMarshaller, metadata, channelMethod, channelInstance);
                    }
                    else if (grpcMethodMethod.Type == MethodType.ServerStreaming)
                    {
                        var addMethod = _serviceBinderAddServerStreamingMethod
                            .MakeGenericMethod(message.RequestType, message.ResponseType)
                            .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, IList<object>, MethodInfo, object>>();
                        addMethod(binder, grpcMethodMethod, metadata, channelMethod, channelInstance);
                    }
                    else if (grpcMethodMethod.Type == MethodType.DuplexStreaming)
                    {
                        var addMethod = _serviceBinderAddDuplexStreamingMethod
                            .MakeGenericMethod(message.HeaderRequestType ?? typeof(Message), message.RequestType.GenericTypeArguments[0], message.ResponseType)
                            .CreateDelegate<Action<IServiceMethodBinder<TService>, IMethod, object?, IList<object>, MethodInfo, object>>();
                        addMethod(binder, grpcMethodMethod, requestHeaderMarshaller, metadata, channelMethod, channelInstance);
                    }
                    else
                    {
                        throw new NotImplementedException("{0} operation is not implemented.".FormatWith(grpcMethodMethod.Type));
                    }
                }
            }
        }

        private static void AddUnaryMethod<TRequest, TResponse>(
            IServiceMethodBinder<TService> binder,
            IMethod grpcMethod,
            IList<object> metadata,
            MethodInfo channelMethod,
            object channelInstance)
            where TRequest : class
            where TResponse : class
        {
            var method = (Method<TRequest, TResponse>)grpcMethod;
            var handler = channelMethod.CreateDelegate<Func<TService, TRequest, ServerCallContext, Task<TResponse>>>(channelInstance);
            binder.AddUnaryMethod(method, metadata, handler);
        }

        private static void AddClientStreamingMethod<TRequestHeader, TRequest, TResponse>(
            IServiceMethodBinder<TService> binder,
            IMethod grpcMethod,
            object? requestHeaderMarshaller,
            IList<object> metadata,
            MethodInfo channelMethod,
            object channelInstance)
            where TRequestHeader : class
            where TResponse : class
        {
            var method = (Method<Message<TRequest>, TResponse>)grpcMethod;
            var handler = channelMethod.CreateDelegate<Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>>>(channelInstance);
            binder.AddClientStreamingMethod(method, (Marshaller<TRequestHeader>?)requestHeaderMarshaller, metadata, handler);
        }

        private static void AddServerStreamingMethod<TRequest, TResponse>(
            IServiceMethodBinder<TService> binder,
            object grpcMethod,
            IList<object> metadata,
            MethodInfo channelMethod,
            object channelInstance)
            where TRequest : class
            where TResponse : class
        {
            var method = (Method<TRequest, TResponse>)grpcMethod;
            var handler = channelMethod.CreateDelegate<Func<TService, TRequest, IServerStreamWriter<TResponse>, ServerCallContext, Task>>(channelInstance);
            binder.AddServerStreamingMethod(method, metadata, handler);
        }

        private static void AddDuplexStreamingMethod<TRequestHeader, TRequest, TResponse>(
            IServiceMethodBinder<TService> binder,
            object grpcMethod,
            object? requestHeaderMarshaller,
            IList<object> metadata,
            MethodInfo channelMethod,
            object channelInstance)
            where TRequestHeader : class
            where TResponse : class
        {
            var method = (Method<Message<TRequest>, TResponse>)grpcMethod;
            var handler = channelMethod.CreateDelegate<Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, Task>>(channelInstance);
            binder.AddDuplexStreamingMethod(method, (Marshaller<TRequestHeader>?)requestHeaderMarshaller, metadata, handler);
        }

        private static IList<object> GetMethodMetadata(Type serviceInstanceType, MethodInfo serviceInstanceMethod)
        {
            // https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs
            var metadata = new List<object>();

            // Add type metadata first so it has a lower priority
            metadata.AddRange(serviceInstanceType.GetCustomAttributes(inherit: true));

            // Add method metadata last so it has a higher priority
            metadata.AddRange(serviceInstanceMethod.GetCustomAttributes(inherit: true));

            return metadata;
        }

        private IList<object> TryGetMethodMetadata(Type interfaceType, MethodInfo operation)
        {
            if (_serviceInstanceType == null)
            {
                return Array.Empty<object>();
            }

            var serviceInstanceMethod = ReflectionTools.ImplementationOfMethod(_serviceInstanceType, interfaceType, operation);
            return GetMethodMetadata(_serviceInstanceType, serviceInstanceMethod);
        }
    }
}
