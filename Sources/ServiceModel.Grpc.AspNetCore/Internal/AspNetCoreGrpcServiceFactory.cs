using System.Collections.Generic;
using System.Reflection;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using ServiceModel.Grpc.Configuration;
using ServiceModel.Grpc.Hosting;
using ServiceModel.Grpc.Internal;

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

        protected override void AddUnaryServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<UnaryServerMethod<TService, TRequest, TResponse>>();
            _context.AddUnaryMethod(method, metadata, invoker);
        }

        protected override void AddClientStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<ClientStreamingServerMethod<TService, TRequest, TResponse>>();
            _context.AddClientStreamingMethod(method, metadata, invoker);
        }

        protected override void AddServerStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<ServerStreamingServerMethod<TService, TRequest, TResponse>>();
            _context.AddServerStreamingMethod(method, metadata, invoker);
        }

        protected override void AddDuplexStreamingServerMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServiceCallInfo callInfo)
        {
            var metadata = GetMethodMetadata(callInfo.ServiceInstanceMethod);
            var invoker = callInfo.ChannelMethod.CreateDelegate<DuplexStreamingServerMethod<TService, TRequest, TResponse>>();
            _context.AddDuplexStreamingMethod(method, metadata, invoker);
        }

        private static IList<object> GetMethodMetadata(MethodInfo serviceInstanceMethod)
        {
            // https://github.com/grpc/grpc-dotnet/blob/master/src/Grpc.AspNetCore.Server/Model/Internal/ProviderServiceBinder.cs
            var metadata = new List<object>();

            // Add type metadata first so it has a lower priority
            metadata.AddRange(typeof(TService).GetCustomAttributes(inherit: true));
            
            // Add method metadata last so it has a higher priority
            metadata.AddRange(serviceInstanceMethod.GetCustomAttributes(inherit: true));

            return metadata;
        }
    }
}
