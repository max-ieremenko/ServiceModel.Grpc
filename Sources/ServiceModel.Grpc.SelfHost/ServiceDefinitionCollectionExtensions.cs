using System;
using ServiceModel.Grpc;
using ServiceModel.Grpc.SelfHost.Internal;

//// ReSharper disable CheckNamespace
namespace Grpc.Core
//// ReSharper restore CheckNamespace
{
    public static class ServiceDefinitionCollectionExtensions
    {
        public static void AddServiceModelTransient<TService>(
            this Server.ServiceDefinitionCollection services,
            Func<TService> serviceFactory,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            services.AssertNotNull(nameof(services));
            serviceFactory.AssertNotNull(nameof(serviceFactory));

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

        public static void AddServiceModelSingleton<TService>(
            this Server.ServiceDefinitionCollection services,
            TService service,
            Action<ServiceModelGrpcServiceOptions> configure = null)
        {
            AddServiceModelTransient(services, () => service, configure);
        }
    }
}
