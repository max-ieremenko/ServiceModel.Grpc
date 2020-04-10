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
        private readonly UnaryServerMethod<TService, TRequest, TResponse> _invoker;

        public UnaryServerCallHandler(
            Func<TService> serviceFactory,
            UnaryServerMethod<TService, TRequest, TResponse> invoker)
        {
            _serviceFactory = serviceFactory;
            _invoker = invoker;
        }

        public Task<TResponse> Handle(TRequest request, ServerCallContext context)
        {
            var service = _serviceFactory();
            return _invoker(service, request, context);
        }
    }
}
