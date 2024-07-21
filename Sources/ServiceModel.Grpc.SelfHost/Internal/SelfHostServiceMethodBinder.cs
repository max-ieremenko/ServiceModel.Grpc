// <copyright>
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
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Hosting.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.SelfHost.Internal;

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
        IMethod method,
        Func<IOperationDescriptor> getDescriptor,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, Task<TResponse>> handler)
        where TRequest : class
        where TResponse : class
    {
        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getDescriptor);
        ValidateFilterFactoryConfiguration(filterHandlerFactory);

        var invoker = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, handler, filterHandlerFactory);
        _builder.AddMethod((Method<TRequest, TResponse>)method, invoker.Handle);
    }

    public void AddClientStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponse>(
        IMethod method,
        Func<IOperationDescriptor> getDescriptor,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, Task<TResponse>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponse : class
    {
        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getDescriptor);
        ValidateFilterFactoryConfiguration(filterHandlerFactory);

        var invoker = new ClientStreamingServerCallHandler<TService, TRequestHeader, TRequest, TRequestValue, TResponse>(
            _serviceFactory,
            handler,
            method,
            filterHandlerFactory);
        _builder.AddMethod((Method<TRequest, TResponse>)method, invoker.Handle);
    }

    public void AddServerStreamingMethod<TRequest, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<IOperationDescriptor> getDescriptor,
        IList<object> metadata,
        Func<TService, TRequest, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequest : class
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getDescriptor);
        ValidateFilterFactoryConfiguration(filterHandlerFactory);

        var invoker = new ServerStreamingServerCallHandler<TService, TRequest, TResponseHeader, TResponse, TResponseValue>(
            _serviceFactory,
            handler,
            method,
            filterHandlerFactory);
        _builder.AddMethod((Method<TRequest, TResponse>)method, invoker.Handle);
    }

    public void AddDuplexStreamingMethod<TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
        IMethod method,
        Func<IOperationDescriptor> getDescriptor,
        IList<object> metadata,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> handler)
        where TRequestHeader : class
        where TRequest : class, IMessage<TRequestValue>
        where TResponseHeader : class
        where TResponse : class, IMessage<TResponseValue>, new()
    {
        var filterHandlerFactory = _filterRegistration.CreateHandlerFactory(metadata, getDescriptor);
        ValidateFilterFactoryConfiguration(filterHandlerFactory);

        var invoker = new DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>(
            _serviceFactory,
            handler,
            method,
            filterHandlerFactory);
        _builder.AddMethod((Method<TRequest, TResponse>)method, invoker.Handle);
    }

    private void ValidateFilterFactoryConfiguration(ServerCallFilterHandlerFactory? filterHandlerFactory)
    {
        if (filterHandlerFactory != null && filterHandlerFactory.ServiceProvider == null)
        {
            var message = $@"Server filters require ServiceProvider instance. Share your IServiceProvider via service configuration:
Server.Services.AddServiceModel...<{typeof(TService).Name}>(options => options.ServiceProvider = [your provider here]);";
            throw new NotSupportedException(message);
        }
    }
}