using System;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceModel.Grpc;
using ServiceModel.Grpc.AspNetCore.Internal;

//// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
//// ReSharper restore CheckNamespace
{
    public static class ServiceCollectionExtensions
    {
        public static IGrpcServerBuilder AddServiceModelGrpc(
            this IServiceCollection services,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            services.AssertNotNull(nameof(services));

            if (configure != null)
            {
                services.Configure(configure);
            }

            var builder = services.AddGrpc();
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IServiceMethodProvider<>), typeof(ServiceModelServiceMethodProvider<>)));
            return builder;
        }

        public static IGrpcServerBuilder AddServiceModelGrpcServiceOptions<TService>(
            this IGrpcServerBuilder builder,
            Action<ServiceModelGrpcServiceOptions<TService>> configure)
            where TService : class
        {
            builder.AssertNotNull(nameof(builder));

            AddServiceModelGrpcServiceOptions(builder.Services, configure);
            return builder;
        }

        public static IServiceCollection AddServiceModelGrpcServiceOptions<TService>(
            this IServiceCollection services,
            Action<ServiceModelGrpcServiceOptions<TService>> configure)
            where TService : class
        {
            services.AssertNotNull(nameof(services));
            configure.AssertNotNull(nameof(configure));

            services.Configure(configure);
            return services;
        }
    }
}
