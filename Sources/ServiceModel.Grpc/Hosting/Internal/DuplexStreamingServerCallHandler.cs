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
using ServiceModel.Grpc.Filters.Internal;
using ServiceModel.Grpc.Internal;

namespace ServiceModel.Grpc.Hosting.Internal;

internal sealed class DuplexStreamingServerCallHandler<TService, TRequestHeader, TRequest, TRequestValue, TResponseHeader, TResponse, TResponseValue>
    where TRequestHeader : class
    where TRequest : class, IMessage<TRequestValue>
    where TResponseHeader : class
    where TResponse : class, IMessage<TResponseValue>, new()
{
    private readonly Func<TService> _serviceFactory;
    private readonly Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponseValue?>)>> _invoker;
    private readonly Marshaller<TRequestHeader>? _requestHeaderMarshaller;
    private readonly Marshaller<TResponseHeader>? _responseHeaderMarshaller;
    private readonly ServerCallFilterHandlerFactory? _filterHandlerFactory;
    private readonly Func<IServerFilterContextInternal, ValueTask> _filterLastAsync;

    public DuplexStreamingServerCallHandler(
        Func<TService> serviceFactory,
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> invoker,
        IMethod method,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
    {
        var grpcMethod = (GrpcMethod<TRequestHeader, TRequest, TResponseHeader, TResponse>)method;

        _serviceFactory = serviceFactory;
        _invoker = invoker;
        _requestHeaderMarshaller = grpcMethod.RequestHeaderMarshaller;
        _responseHeaderMarshaller = grpcMethod.ResponseHeaderMarshaller;
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
        Func<TService, TRequestHeader?, IAsyncEnumerable<TRequestValue?>, ServerCallContext, ValueTask<(TResponseHeader? Header, IAsyncEnumerable<TResponseValue?> Response)>> invoker,
        IMethod method,
        ServerCallFilterHandlerFactory? filterHandlerFactory)
        : this(null!, invoker, method, filterHandlerFactory)
    {
    }

    public Task Handle(
        IAsyncStreamReader<TRequest> input,
        IServerStreamWriter<TResponse> output,
        ServerCallContext context) =>
        Handle(_serviceFactory(), input, output, context);

    public Task Handle(
        TService service,
        IAsyncStreamReader<TRequest> input,
        IServerStreamWriter<TResponse> output,
        ServerCallContext context)
    {
        TRequestHeader? header = null;
        if (_requestHeaderMarshaller != null)
        {
            header = CompatibilityTools.DeserializeMethodInputHeader(_requestHeaderMarshaller, context.RequestHeaders);
        }

        var request = ServerChannelAdapter.ReadClientStream<TRequest, TRequestValue>(input, context);

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
        IAsyncEnumerable<TRequestValue?> input,
        IServerStreamWriter<TResponse> output,
        ServerCallContext context)
    {
        var handler = _filterHandlerFactory!.CreateHandler(service!, context);
        handler.Context.RequestInternal.SetRaw(inputHeader, input);
        await handler.InvokeAsync(_filterLastAsync).ConfigureAwait(false);

        var (rawHeader, rawData) = handler.Context.ResponseInternal.GetRaw();

        var outputHeader = (TResponseHeader?)rawHeader;
        var outputData = (IAsyncEnumerable<TResponseValue?>)rawData!;
        var result = new ValueTask<(TResponseHeader?, IAsyncEnumerable<TResponseValue?>)>((outputHeader, outputData));

        await ServerChannelAdapter.WriteServerStreamingResult(result, _responseHeaderMarshaller, output, context).ConfigureAwait(false);
    }

    private async ValueTask FilterLastAsync(IServerFilterContextInternal context)
    {
        var service = (TService)context.ServiceInstance;
        var (inputHeader, inputStream) = context.RequestInternal.GetRaw();

        var (responseHeader, responseStream) = await _invoker(
                service,
                (TRequestHeader?)inputHeader,
                (IAsyncEnumerable<TRequestValue?>)inputStream!,
                context.ServerCallContext)
            .ConfigureAwait(false);
        context.ResponseInternal.SetRaw(responseHeader, responseStream);
    }
}