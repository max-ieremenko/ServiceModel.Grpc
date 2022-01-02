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
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Filters.Internal;

namespace ServiceModel.Grpc.Hosting.Internal
{
    internal sealed class ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponse>
        where TRequestHeader : class
        where TResponse : class
    {
        private readonly ServerCallFilterHandlerFactory? _filterHandlerFactory;
        private readonly Func<TService> _serviceFactory;
        private readonly Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> _invoker;
        private readonly Marshaller<TRequestHeader>? _requestHeaderMarshaller;
        private readonly Func<IServerFilterContextInternal, ValueTask> _filterLastAsync;

        public ClientStreamingServerCallHandler(
            Func<TService> serviceFactory,
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            ServerCallFilterHandlerFactory? filterHandlerFactory)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
            _requestHeaderMarshaller = requestHeaderMarshaller;
            _filterHandlerFactory = filterHandlerFactory;

            if (filterHandlerFactory == null)
            {
                _filterLastAsync = null!;
            }
            else
            {
                _filterLastAsync = FilterLastAsync;
            }
        }

        public ClientStreamingServerCallHandler(
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, Task<TResponse>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            ServerCallFilterHandlerFactory? filterHandlerFactory)
            : this(null!, invoker, requestHeaderMarshaller, filterHandlerFactory)
        {
        }

        public Task<TResponse> Handle(
            IAsyncStreamReader<Message<TRequest>> stream,
            ServerCallContext serverCallContext)
        {
            return Handle(_serviceFactory(), stream, serverCallContext);
        }

        public Task<TResponse> Handle(
            TService service,
            IAsyncStreamReader<Message<TRequest>> stream,
            ServerCallContext serverCallContext)
        {
            TRequestHeader? header = null;
            if (_requestHeaderMarshaller != null)
            {
                header = CompatibilityTools.DeserializeMethodInputHeader(_requestHeaderMarshaller, serverCallContext.RequestHeaders);
            }

            var request = ServerChannelAdapter.ReadClientStream(stream, serverCallContext);

            if (_filterHandlerFactory == null)
            {
                return _invoker(service, header, request, serverCallContext);
            }

            return HandleWithFilter(service, header, request, serverCallContext);
        }

        private async Task<TResponse> HandleWithFilter(TService service, TRequestHeader? header, IAsyncEnumerable<TRequest> stream, ServerCallContext context)
        {
            var handler = _filterHandlerFactory!.CreateHandler(service!, context);
            handler.Context.RequestInternal.SetRaw(header!, stream);

            await handler.InvokeAsync(_filterLastAsync).ConfigureAwait(false);

            return (TResponse)handler.Context.ResponseInternal.GetRaw().Response;
        }

        private async ValueTask FilterLastAsync(IServerFilterContextInternal context)
        {
            var service = (TService)context.ServiceInstance;
            var (header, stream) = context.RequestInternal.GetRaw();
            var response = await _invoker(service, (TRequestHeader?)header, (IAsyncEnumerable<TRequest>)stream!, context.ServerCallContext).ConfigureAwait(false);
            context.ResponseInternal.SetRaw(response, null);
        }
    }
}