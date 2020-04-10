using Grpc.AspNetCore.Server.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceModel.Grpc.AspNetCore.Internal
{
    internal sealed class ServiceModelServiceMethodProvider<TService> : IServiceMethodProvider<TService>
        where TService : class
    {
        private readonly ServiceModelGrpcServiceOptions _rootConfiguration;
        private readonly ServiceModelGrpcServiceOptions<TService> _serviceConfiguration;
        private readonly ILogger<ServiceModelServiceMethodProvider<TService>> _logger;

        public ServiceModelServiceMethodProvider(
            IOptions<ServiceModelGrpcServiceOptions> rootConfiguration,
            IOptions<ServiceModelGrpcServiceOptions<TService>> serviceConfiguration,
            ILogger<ServiceModelServiceMethodProvider<TService>> logger)
        {
            rootConfiguration.AssertNotNull(nameof(rootConfiguration));
            serviceConfiguration.AssertNotNull(nameof(serviceConfiguration));
            logger.AssertNotNull(nameof(logger));

            _rootConfiguration = rootConfiguration.Value;
            _serviceConfiguration = serviceConfiguration.Value;
            _logger = logger;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var marshallerFactory = _serviceConfiguration.MarshallerFactory ?? _rootConfiguration.DefaultMarshallerFactory;
            var log = new LogAdapter(_logger);

            var factory = new AspNetCoreGrpcServiceFactory<TService>(log, context, marshallerFactory);
            factory.Bind();
        }
    }
}
