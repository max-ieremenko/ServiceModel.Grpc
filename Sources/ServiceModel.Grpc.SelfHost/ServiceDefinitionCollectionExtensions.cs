using System;
using ServiceModel.Grpc;
using ServiceModel.Grpc.Internal;
using ServiceModel.Grpc.SelfHost.Internal;

//// ReSharper disable CheckNamespace
namespace Grpc.Core
//// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Provides a set of methods to simplify ServiceModel.Grpc services registration.
    /// </summary>
    public static class ServiceDefinitionCollectionExtensions
    {
        /// <summary>
        /// Registers a ServiceModel.Grpc service (one instance per-call) in the <see cref="Server.ServiceDefinitionCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
        /// <param name="serviceFactory">Method which creates a service instance.</param>
        /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
        public static void AddServiceModelTransient<TService>(
            this Server.ServiceDefinitionCollection services,
            Func<TService> serviceFactory,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            services.AssertNotNull(nameof(services));
            serviceFactory.AssertNotNull(nameof(serviceFactory));

            if (ServiceContract.IsNativeGrpcService(typeof(TService)))
            {
                throw new InvalidOperationException("{0} is native grpc service.".FormatWith(typeof(TService).FullName));
            }

            ServiceModelGrpcServiceOptions options = null;
            if (configure != null)
            {
                options = new ServiceModelGrpcServiceOptions();
                configure(options);
            }

            var builder = ServerServiceDefinition.CreateBuilder();

            var factory = new SelfHostGrpcServiceFactory<TService>(
                new LogAdapter(options?.Logger),
                options?.MarshallerFactory,
                serviceFactory,
                builder);

            factory.Bind();

            var definition = builder.Build();
            if (options?.ConfigureServiceDefinition != null)
            {
                definition = options.ConfigureServiceDefinition(definition);
            }

            services.Add(definition);
        }

        /// <summary>
        /// Registers a ServiceModel.Grpc service (one instance for all calls) in the <see cref="Server.ServiceDefinitionCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The implementation type of ServiceModel.Grpc service.</typeparam>
        /// <param name="services">The <see cref="Server.ServiceDefinitionCollection"/>.</param>
        /// <param name="service">The service instance.</param>
        /// <param name="configure">The optional configuration action to provide a configuration the service.</param>
        public static void AddServiceModelSingleton<TService>(
            this Server.ServiceDefinitionCollection services,
            TService service,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            AddServiceModelTransient(services, () => service, configure);
        }
    }
}
