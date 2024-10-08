﻿// <copyright>
// Copyright Max Ieremenko
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

using Grpc.Core;
using ServiceModel.Grpc.Channel;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Hosting.Internal;

internal sealed class ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse, TResponseValue>
    where TRequest : class
    where TResponseHeader : class
    where TResponse : class, IMessage<TResponseValue>, new()
{
    private readonly Func<TService> _serviceFactory;
    private readonly Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponseValue?>)>> _invoker;
    private readonly Marshaller<TResponseHeader>? _responseHeaderMarshaller;
    private readonly ServerCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly Func<IServerFilterContextInternal, ValueTask> _filterLastAsync;

    public ServerStreamingServerCallHandler(
        Func<TService> serviceFactory,
        Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> invoker,
        IMethod method,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
    {
        _serviceFactory = serviceFactory;
        _invoker = invoker;
        _responseHeaderMarshaller = ((GrpcMethod<Message, TRequest, TResponseHeader, TResponse>)method).ResponseHeaderMarshaller;
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

    public ServerStreamingServerCallHandler(
        Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> invoker,
        IMethod method,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
        : this(null!, invoker, method, filterHandlerFactory)
    {
    }

    public Task Handle(TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context) =>
        Handle(_serviceFactory(), request, stream, context);

    public Task Handle(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
    {
        if (_filterHandlerFactory == null)
        {
            var result = _invoker(service, request, context);
            return ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, stream, context);
        }

        return HandleWithFilter(service, request, stream, context);
    }

    private async Task HandleWithFilter(TService service, TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context)
    {
        var handler = _filterHandlerFactory!.CreateHandler(service!, context);
        handler.Context.RequestInternal.SetRaw(request, null);

        await handler.InvokeAsync(_filterLastAsync).ConfigureAwait(false);

        var (rawHeader, rawData) = handler.Context.ResponseInternal.GetRaw();

        var header = (TResponseHeader?)rawHeader;
        var data = (IAsyncEnumerable<TResponseValue?>)rawData!;
        var result = new ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponseValue?>)>((header, data));

        await ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, stream, context).ConfigureAwait(false);
    }

    private async ValueTask FilterLastAsync(IServerFilterContextInternal context)
    {
        var service = (TService)context.ServiceInstance;
        var request = (TRequest)context.RequestInternal.GetRaw().Request!;
        var (header, stream) = await _invoker(service, request, context.ServerCallContext).ConfigureAwait(false);
        context.ResponseInternal.SetRaw(header, stream);
    }
}