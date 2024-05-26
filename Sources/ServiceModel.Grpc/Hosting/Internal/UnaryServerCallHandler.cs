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
using System.Threading.Tasks;
using Grpc.Core;
using ServiceModel.Grpc.Filters.Internal;

namespace ServiceModel.Grpc.Hosting.Internal;

internal sealed class UnaryServerCallHandler<TService, TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    private readonly ServerCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly Func<TService> _serviceFactory;
    private readonly Func<TService, TRequest, ServerCallContext, Task<TResponse>> _invoker;
    private readonly Func<IServerFilterContextInternal, ValueTask> _filterLastAsync;

    public UnaryServerCallHandler(
        Func<TService> serviceFactory,
        Func<TService, TRequest, ServerCallContext, Task<TResponse>> invoker,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
    {
        _serviceFactory = serviceFactory;
        _invoker = invoker;
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

    public UnaryServerCallHandler(
        Func<TService, TRequest, ServerCallContext, Task<TResponse>> invoker,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
        : this(null!, invoker, filterHandlerFactory)
    {
    }

    public Task<TResponse> Handle(TRequest request, ServerCallContext context) => Handle(_serviceFactory(), request, context);

    public Task<TResponse> Handle(TService service, TRequest request, ServerCallContext context)
    {
        if (_filterHandlerFactory == null)
        {
            return _invoker(service, request, context);
        }

        return HandleWithFilter(service, request, context);
    }

    private async Task<TResponse> HandleWithFilter(TService service, TRequest request, ServerCallContext context)
    {
        var handler = _filterHandlerFactory!.CreateHandler(service!, context);
        handler.Context.RequestInternal.SetRaw(request, null);

        await handler.InvokeAsync(_filterLastAsync).ConfigureAwait(false);

        return (TResponse)handler.Context.ResponseInternal.GetRaw().Response;
    }

    private async ValueTask FilterLastAsync(IServerFilterContextInternal context)
    {
        var service = (TService)context.ServiceInstance;
        var request = (TRequest)context.RequestInternal.GetRaw().Request!;
        var response = await _invoker(service, request, context.ServerCallContext).ConfigureAwait(false);
        context.ResponseInternal.SetRaw(response, null);
    }
}