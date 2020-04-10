using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class DuplexStreamingServerCallHandler<TService, TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly Func<TService> _serviceFactory;
        private readonly DuplexStreamingServerMethod<TService, TRequest, TResponse> _invoker;

        public DuplexStreamingServerCallHandler(
            Func<TService> serviceFactory,
            DuplexStreamingServerMethod<TService, TRequest, TResponse> invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        public Task Handle(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, requestStream, responseStream, context);
        }
    }
}