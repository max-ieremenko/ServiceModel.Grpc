using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace WcfServer;

internal sealed class DependencyInjectionServiceHost : ServiceHost
{
    public DependencyInjectionServiceHost(IServiceProvider serviceProvider, Type serviceType, params Uri[] baseAddresses)
        : base(serviceType, baseAddresses)
    {
        foreach (var contractDescription in ImplementedContracts.Values)
        {
            var instanceProvider = new InstanceProvider(serviceProvider, serviceType);
            var contractBehavior = new ContractBehavior(instanceProvider);

            contractDescription.Behaviors.Add(contractBehavior);
        }
    }

    private sealed class ContractBehavior : IContractBehavior
    {
        private readonly IInstanceProvider _instanceProvider;

        public ContractBehavior(IInstanceProvider instanceProvider)
        {
            _instanceProvider = instanceProvider;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = _instanceProvider;
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
    }

    private sealed class InstanceProvider : IInstanceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _serviceType;

        public InstanceProvider(IServiceProvider serviceProvider, Type serviceType)
        {
            _serviceProvider = serviceProvider;
            _serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext) => _serviceProvider.GetRequiredService(_serviceType);

        public object GetInstance(InstanceContext instanceContext, Message message) => _serviceProvider.GetRequiredService(_serviceType);

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
