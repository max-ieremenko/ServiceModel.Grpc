using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class ServerStreamingServerCallHandler<TService, TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ServerStreamingServerMethod<TService, TRequest, TResponse> _invoker;

        public ServerStreamingServerCallHandler(
            Func<TService> serviceFactory,
            ServerStreamingServerMethod<TService, TRequest, TResponse> invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        public Task Handle(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, request, responseStream, context);
        }
    }
}