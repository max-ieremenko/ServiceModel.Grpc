using System.Collections.Generic;
using System.Reflection;
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
            ILogger logger,
            ServiceMethodProviderContext<TService> context,
            IMarshallerFactory marshallerFactory)
            : base(logger, marshallerFactory)
        {
            _context = context;
        }

        protected override void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, MethodInfo serviceMethod, GrpcServiceBuilder serviceBuilder)
        {
            var metadata = GetMethodMetadata(serviceMethod);
            var invoker = serviceBuilder.CreateCall<UnaryServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddUnaryMethod(method, metadata, invoker);
        }

        protected override void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, MethodInfo serviceMethod, GrpcServiceBuilder serviceBuilder)
        {
            var metadata = GetMethodMetadata(serviceMethod);
            var invoker = serviceBuilder.CreateCall<ClientStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddClientStreamingMethod(method, metadata, invoker);
        }

        protected override void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, MethodInfo serviceMethod, GrpcServiceBuilder serviceBuilder)
        {
            var metadata = GetMethodMetadata(serviceMethod);
            var invoker = serviceBuilder.CreateCall<ServerStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddServerStreamingMethod(method, metadata, invoker);
        }

        protected override void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, MethodInfo serviceMethod, GrpcServiceBuilder serviceBuilder)
        {
            var metadata = GetMethodMetadata(serviceMethod);
            var invoker = serviceBuilder.CreateCall<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(method.Name);
            _context.AddDuplexStreamingMethod(method, metadata, invoker);
        }

        private static IList<object> GetMethodMetadata(MethodInfo serviceMethod)
        {
            // https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs
            var metadata = new List<object>();

            // Add type metadata first so it has a lower priority
            metadata.AddRange(typeof(TService).GetCustomAttributes(inherit: true));
            
            // Add method metadata last so it has a higher priority
            metadata.AddRange(serviceMethod.GetCustomAttributes(inherit: true));

            return metadata;
        }
    }
}
