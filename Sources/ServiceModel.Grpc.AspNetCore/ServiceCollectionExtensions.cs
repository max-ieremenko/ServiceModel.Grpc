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
    /// <summary>
    /// Provides a set of methods to simplify ServiceModel.Grpc services registration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Enables ServiceModel.Grpc services for the specific <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configure">The optional configuration action to provide default configuration for all ServiceModel.Grpc services.</param>
        /// <returns>The <see cref="IGrpcServerBuilder"/>.</returns>
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

        /// <summary>
        /// Registers a configuration for the specific ServiceModel.Grpc service in the <see cref="IGrpcServerBuilder"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="builder">The <see cref="IGrpcServerBuilder"/>.</param>
        /// <param name="configure">The configuration action to provide a configuration the specific ServiceModel.Grpc service.</param>
        /// <returns><see cref="IGrpcServerBuilder"/>.</returns>
        public static IGrpcServerBuilder AddServiceModelGrpcServiceOptions<TService>(
            this IGrpcServerBuilder builder,
            Action<ServiceModelGrpcServiceOptions<TService>> configure)
            where TService : class
        {
            builder.AssertNotNull(nameof(builder));

            AddServiceModelGrpcServiceOptions(builder.Services, configure);
            return builder;
        }

        /// <summary>
        /// Registers a configuration for the specific ServiceModel.Grpc service in the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configure">The configuration action to provide a configuration the specific ServiceModel.Grpc service.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
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
