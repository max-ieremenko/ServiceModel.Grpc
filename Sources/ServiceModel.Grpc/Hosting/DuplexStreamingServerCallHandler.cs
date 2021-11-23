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

namespace ServiceModel.Grpc.Hosting
{
    internal sealed class DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TResponseHeader, TResponse>
        where TRequestHeader : class
        where TResponseHeader : class
    {
        private readonly Func<TService> _serviceFactory;
        private readonly Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponse>)>> _invoker;
        private readonly Marshaller<TRequestHeader>? _requestHeaderMarshaller;
        private readonly Marshaller<TResponseHeader>? _responseHeaderMarshaller;
        private readonly ServerCallFilterHandlerFactory? _filterHandlerFactory;
        private readonly Func<IServerFilterContextInternal, ValueTask> _filterLastAsync;

        public DuplexStreamingServerCallHandler(
            Func<TService> serviceFactory,
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            Marshaller<TResponseHeader>? responseHeaderMarshaller,
            ServerCallFilterHandlerFactory? filterHandlerFactory)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
            _requestHeaderMarshaller = requestHeaderMarshaller;
            _responseHeaderMarshaller = responseHeaderMarshaller;
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

        public DuplexStreamingServerCallHandler(
            Func<TService, TRequestHeader?, IAsyncEnumerable<TRequest>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponse> Response)>> invoker,
            Marshaller<TRequestHeader>? requestHeaderMarshaller,
            Marshaller<TResponseHeader>? responseHeaderMarshaller,
            ServerCallFilterHandlerFactory? filterHandlerFactory)
            : this(null!, invoker, requestHeaderMarshaller, responseHeaderMarshaller, filterHandlerFactory)
        {
        }

        public Task Handle(
            IAsyncStreamReader<Message<TRequest>> input,
            IServerStreamWriter<Message<TResponse>> output,
            ServerCallContext context)
        {
            return Handle(_serviceFactory(), input, output, context);
        }

        public Task Handle(
            TService service,
            IAsyncStreamReader<Message<TRequest>> input,
            IServerStreamWriter<Message<TResponse>> output,
            ServerCallContext context)
        {
            TRequestHeader? header = null;
            if (_requestHeaderMarshaller != null)
            {
                header = CompatibilityTools.DeserializeMethodInputHeader(_requestHeaderMarshaller, context.RequestHeaders);
            }

            var request = ServerChannelAdapter.ReadClientStream(input, context);

            if (_filterHandlerFactory == null)
            {
                var result = _invoker(service, header, request, context);
                return ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, output, context);
            }

            return HandleWithFilter(service, header, request, output, context);
        }

        private async Task HandleWithFilter(
            TService service,
            TRequestHeader? inputHeader,
            IAsyncEnumerable<TRequest> input,
            IServerStreamWriter<Message<TResponse>> output,
            ServerCallContext context)
        {
            var handler = _filterHandlerFactory!.CreateHandler(service!, context);
            handler.Context.RequestInternal.SetRaw(inputHeader, input);
            await handler.InvokeAsync(_filterLastAsync).ConfigureAwait(false);

            var (rawHeader, rawData) = handler.Context.ResponseInternal.GetRaw();

            var outputHeader = (TResponseHeader?)rawHeader;
            var outputData = (IAsyncEnumerable<TResponse>)rawData!;
            var result = new ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponse>)>((outputHeader, outputData));

            await ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, output, context);
        }

        private async ValueTask FilterLastAsync(IServerFilterContextInternal context)
        {
            var service = (TService)context.ServiceInstance;
            var (inputHeader, inputStream) = context.RequestInternal.GetRaw();

            var (responseHeader, responseStream) = await _invoker(service, (TRequestHeader?)inputHeader, (IAsyncEnumerable<TRequest>)inputStream!, context.ServerCallContext).ConfigureAwait(false);
            context.ResponseInternal.SetRaw(responseHeader, responseStream);
        }
    }
}