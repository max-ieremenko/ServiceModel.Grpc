using Contract;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Service;

namespace WcfServer;

internal sealed class WcfHost : ServiceHost, IHostedService
{
    private readonly ServiceHost _host;

    public WcfHost(IServiceProvider serviceProvider)
    {
        _host = new DependencyInjectionServiceHost(
             serviceProvider,
             typeof(PersonService),
             new Uri(SharedConfiguration.WcfPersonServiceLocation));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // if receive System.ServiceModel.AddressAccessDeniedException on start-up
        // re-start your visual studio with administrator permissions "Run as administrator"
        _host.Open();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _host.Close();
        return Task.CompletedTask;
    }
}