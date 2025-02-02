using Contract;
using Microsoft.Extensions.Hosting;
using Service;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace WcfServer;

internal sealed class WcfHost : IHostedService
{
    private readonly ServiceHost _host;

    public WcfHost()
    {
        _host = new ServiceHost(typeof(DebugService), new Uri(SharedConfiguration.WcfDebugServiceLocation));
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