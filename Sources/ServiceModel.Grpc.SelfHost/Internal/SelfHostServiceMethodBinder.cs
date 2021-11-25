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
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class SelfHostServiceMethodBinder<TService> : IServiceMethodBinder<TService>
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ServiceMethodFilterRegistration _filterRegistration;
        private readonly ServerServiceDefinition.Builder _builder;

        public SelfHostServiceMethodBinder(
            IMarshallerFactory marshallerFactory,
            Func<TService> serviceFactory,
            ServiceMethodFilterRegistration filterRegistration,
            ServerServiceDefinition.Builder builder)
        {
            MarshallerFactory = marshallerFactory;
            _serviceFactory = serviceFactory;
            _filterRegistration = filterRegistration;
            _builder = builder;
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
            ValidateFilterFactoryConfiguration(filterHandlerFactory);

            var invoker = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler, filterHandlerFactory);
            _builder.AddMethod(method, invoker.Handle);
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
            ValidateFilterFactoryConfiguration(filterHandlerFactory);

            var invoker = new ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponse>(
                _serviceFactory,
                handler,
                requestHeaderMarshaller,
                filterHandlerFactory);
            _builder.AddMethod(method, invoker.Handle);
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
            ValidateFilterFactoryConfiguration(filterHandlerFactory);

            var invoker = new ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse>(
                _serviceFactory,
                handler,
                responseHeaderMarshaller,
                filterHandlerFactory);
            _builder.AddMethod(method, invoker.Handle);
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
            ValidateFilterFactoryConfiguration(filterHandlerFactory);

            var invoker = new DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponseHeader, TResponse>(
                _serviceFactory,
                handler,
                requestHeaderMarshaller,
                responseHeaderMarshaller,
                filterHandlerFactory);
            _builder.AddMethod(method, invoker.Handle);
        }

        private void ValidateFilterFactoryConfiguration(ServerCallFilterHandlerFactory? filterHandlerFactory)
        {
            if (filterHandlerFactory != null && filterHandlerFactory.ServiceProvider == null)
            {
                var message = @"Server filters require ServiceProvider instance. Share your IServiceProvider via service configuration:
Server.Services.AddServiceModel...<{0}>(options => options.ServiceProvider = [your provider here]);";
                throw new NotSupportedException(message.FormatWith(typeof(TService).Name));
            }
        }
    }
}
