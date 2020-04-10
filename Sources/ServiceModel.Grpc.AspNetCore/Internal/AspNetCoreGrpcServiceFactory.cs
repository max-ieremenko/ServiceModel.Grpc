using System;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal.Emit;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class AspNetCoreGrpcServiceFactory<TService> : GrpcServiceFactoryBase<TService>
        where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;

        public AspNetCoreGrpcServiceFactory(
            ILog logger,
            ServiceMethodProviderContext<TService> context,
            IMarshallerFactory marshallerFactory)
            : base(logger, marshallerFactory)
        {
            _context = context;
        }

        protected override void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<UnaryServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddUnaryMethod(method, Array.Empty<object>(), invoker);
        }

        protected override void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<ClientStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddClientStreamingMethod(method, Array.Empty<object>(), invoker);
        }

        protected override void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<ServerStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddServerStreamingMethod(method, Array.Empty<object>(), invoker);
        }

        protected override void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, GrpcServiceBuilder serviceBuilder)
        {
            var invoker = serviceBuilder.CreateCall<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddDuplexStreamingMethod(method, Array.Empty<object>(), invoker);
        }
    }
}
