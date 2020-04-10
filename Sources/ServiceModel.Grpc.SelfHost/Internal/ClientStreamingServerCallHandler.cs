using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class ClientStreamingServerCallHandler<TService, TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ClientStreamingServerMethod<TService, TRequest, TResponse> _invoker;

        public ClientStreamingServerCallHandler(
            Func<TService> serviceFactory,
            ClientStreamingServerMethod<TService, TRequest, TResponse> invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        public Task<TResponse> Handle(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, requestStream, context);
        }
    }
}