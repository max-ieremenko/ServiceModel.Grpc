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
        private readonly ServerStreamingServerMethod _invoker;

        public ServerStreamingServerCallHandler(
            Func<TService> serviceFactory,
            ServerStreamingServerMethod invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        internal delegate Task ServerStreamingServerMethod(
            TService service,
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context);

        public Task Handle(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, request, responseStream, context);
        }
    }
}