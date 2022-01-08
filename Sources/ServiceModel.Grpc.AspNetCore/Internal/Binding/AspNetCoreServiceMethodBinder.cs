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
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;

namespace ServiceModel.Grpc.AspNetCore.Internal.Binding
{
    internal sealed class AspNetCoreServiceMethodBinder<TService> : IServiceMethodBinder<TService>
        where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;
        private readonly ServiceMethodFilterRegistration _filterRegistration;
        private readonly bool _requiresGrpcMarker;

        public AspNetCoreServiceMethodBinder(
            ServiceMethodProviderContext<TService> context,
            IMarshallerFactory marshallerFactory,
            ServiceMethodFilterRegistration filterRegistration,
            bool requiresGrpcMarker)
        {
            _context = context;
            _filterRegistration = filterRegistration;
            _requiresGrpcMarker = requiresGrpcMarker;
            MarshallerFactory = marshallerFactory;
        }

        public IMarshallerFactory MarshallerFactory { get; }

        public void AddUnaryMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            Func<MethodInfo> resolveContractMethodDefinition,
            IList<object> metadata,
            Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
            where TRequest : class
            where TResponse : class
        {
            var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, resolveContractMethodDefinition);
            var invoker = new UnaryServerCallHandler<TService, TRequest, TResponse>(handler, filterHandlerFactory);

            if (_requiresGrpcMarker)
            {
                metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.ContractMethodDefinition ?? resolveContractMethodDefinition());
            }

            _context.AddUnaryMethod(method, metadata, invoker.Handle);
        }

        public void AddClientStreamingMethod<TRequestHeader, TRequest, TResponse>(
            Method<Message<TRequest>, TResponse> method,
            Func<MethodInfo> resolveContractMethodDefinition,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            IList<object> metadata,
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> handler)
            where TRequestHeader : class
            where TResponse : class
        {
            var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, resolveContractMethodDefinition);
            var invoker = new ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponse>(handler, requestHeaderMarshaller, filterHandlerFactory);

            if (_requiresGrpcMarker)
            {
                metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.ContractMethodDefinition ?? resolveContractMethodDefinition());
            }

            _context.AddClientStreamingMethod(method, metadata, invoker.Handle);
        }

        public void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse>(
            Method<TRequest, Message<TResponse>> method,
            Func<MethodInfo> resolveContractMethodDefinition,
            Marshaller<TResponseHeader>? responseHeaderMarshaller,
            IList<object> metadata,
            Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> handler)
            where TRequest : class
            where TResponseHeader : class
        {
            var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, resolveContractMethodDefinition);
            var invoker = new ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse>(handler, responseHeaderMarshaller, filterHandlerFactory);

            if (_requiresGrpcMarker)
            {
                metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.ContractMethodDefinition ?? resolveContractMethodDefinition());
            }

            _context.AddServerStreamingMethod(method, metadata, invoker.Handle);
        }

        public void AddDuplexStreamingMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>(
            Method<Message<TRequest>, Message<TResponse>> method,
            Func<MethodInfo> resolveContractMethodDefinition,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            Marshaller<TResponseHeader>? responseHeaderMarshaller,
            IList<object> metadata,
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> handler)
            where TRequestHeader : class
            where TResponseHeader : class
        {
            var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, resolveContractMethodDefinition);
            var invoker = new DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponseHeader, TResponse>(handler, requestHeaderMarshaller, responseHeaderMarshaller, filterHandlerFactory);

            if (_requiresGrpcMarker)
            {
                metadata = AddServiceModelGrpcMarker(metadata, filterHandlerFactory?.ContractMethodDefinition ?? resolveContractMethodDefinition());
            }

            _context.AddDuplexStreamingMethod(method, metadata, invoker.Handle);
        }

        private static IList<object> AddServiceModelGrpcMarker(IList<object>? metadata, MethodInfo contractMethodDefinition)
        {
            var metadataLength = metadata?.Count ?? 0;
            var result = new object[metadataLength + 1];

            for (var i = 0; i < metadataLength; i++)
            {
                result[i] = metadata![i];
            }

            result[metadataLength] = new ServiceModelGrpcMarker(contractMethodDefinition);
            return result;
        }
    }
}
