// <copyright>
// Copyright 2021 Max Ieremenko
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
using System.Reflection;
using Grpc.Core;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Filters.Internal
{
    internal sealed class ServerCallFilterHandlerFactory
    {
        private readonly Func<object, MethodInfo> _getServiceMethodInfo;
        private readonly MessageProxy _requestMessageProxy;
        private readonly MessageProxy _responseMessageProxy;
        private readonly StreamProxy? _requestStreamProxy;
        private readonly StreamProxy? _responseStreamProxy;
        private MethodInfo? _serviceMethodInfo;

        public ServerCallFilterHandlerFactory(
            IServiceProvider serviceProvider,
            MethodInfo contractMethodDefinition,
            Func<IServiceProvider, IServerFilter>[] filterFactories)
        {
            ServiceProvider = serviceProvider;
            ContractMethodDefinition = contractMethodDefinition;
            FilterFactories = filterFactories;
            _getServiceMethodInfo = GetServiceMethodInfo;

            var proxyFactory = new ProxyFactory(contractMethodDefinition);
            _requestMessageProxy = proxyFactory.RequestProxy;
            _responseMessageProxy = proxyFactory.ResponseProxy;
            _requestStreamProxy = proxyFactory.RequestStreamProxy;
            _responseStreamProxy = proxyFactory.ResponseStreamProxy;
        }

        public IServiceProvider ServiceProvider { get; }

        public MethodInfo ContractMethodDefinition { get; }

        public Func<IServiceProvider, IServerFilter>[] FilterFactories { get; }

        public ServerCallFilterHandler CreateHandler(object service, ServerCallContext context)
        {
            var filters = new IServerFilter[FilterFactories.Length];
            for (var i = 0; i < FilterFactories.Length; i++)
            {
                filters[i] = CreateFilter(FilterFactories[i]);
            }

            var filterContext = new ServerFilterContext(
                service,
                context,
                ServiceProvider,
                ContractMethodDefinition,
                _getServiceMethodInfo,
                new RequestContext(_requestMessageProxy, _requestStreamProxy),
                new ResponseContext(_responseMessageProxy, _responseStreamProxy));

            return new ServerCallFilterHandler(filterContext, filters);
        }

        private IServerFilter CreateFilter(Func<IServiceProvider, IServerFilter> factory)
        {
            IServerFilter? filter;
            try
            {
                filter = factory(ServiceProvider);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create a server filter: {0}. Please check server filter registrations.".FormatWith(ex.Message), ex);
            }

            if (filter == null)
            {
                throw new InvalidOperationException("Server filter factory must not return null. Please check server filter registrations.");
            }

            return filter;
        }

        private MethodInfo GetServiceMethodInfo(object service)
        {
            if (_serviceMethodInfo != null)
            {
                return _serviceMethodInfo;
            }

            _serviceMethodInfo = ReflectionTools.ImplementationOfMethod(
                service.GetType(),
                ContractMethodDefinition.DeclaringType,
                ContractMethodDefinition);
            return _serviceMethodInfo;
        }
    }
}
