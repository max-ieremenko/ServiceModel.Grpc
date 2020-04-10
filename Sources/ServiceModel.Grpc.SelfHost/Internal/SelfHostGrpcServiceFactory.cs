using System;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.SelfHost.Internal
{
    internal sealed class SelfHostGrpcServiceFactory<TService> : GrpcServiceFactoryBase<TService>
    {
        private readonly Func<TService> _serviceFactory;
        private readonly ServerServiceDefinition.Builder _builder;

        public SelfHostGrpcServiceFactory(
            ILog logger,
            IMarshallerFactory marshallerFactory,
            Func<TService> serviceFactory,
            ServerServiceDefinition.Builder builder)
            : base(logger, marshallerFactory)
        {
            _serviceFactory = serviceFactory;
            _builder = builder;
        }

        protected override void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<UnaryServerCallHandler<TService, TRequest, TResponse>.UnaryServerMethod>(method.Name);
            var handler = new UnaryServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<ClientStreamingServerCallHandler<TService, TRequest, TResponse>.ClientStreamingServerMethod>(method.Name);
            var handler = new ClientStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<ServerStreamingServerCallHandler<TService, TRequest, TResponse>.ServerStreamingServerMethod>(method.Name);
            var handler = new ServerStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }

        protected override void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<DuplexStreamingServerCallHandler<TService, TRequest, TResponse>.DuplexStreamingServerMethod>(method.Name);
            var handler = new DuplexStreamingServerCallHandler<TService, TRequest, TResponse>(_serviceFactory, invoker);
            _builder.AddMethod(method, handler.Handle);
        }
    }
}
