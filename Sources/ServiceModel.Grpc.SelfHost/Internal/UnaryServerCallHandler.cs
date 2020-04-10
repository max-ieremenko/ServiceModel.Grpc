using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class UnaryServerCallHandler<TService, TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private readonly Func<TService> _serviceFactory;
        private readonly UnaryServerMethod _invoker;

        public UnaryServerCallHandler(
            Func<TService> serviceFactory,
            UnaryServerMethod invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        internal delegate Task<TResponse> UnaryServerMethod(
            TService service,
            TRequest request,
            ServerCallContext context);

        public Task<TResponse> Handle(TRequest request, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, request, context);
        }
    }
}
