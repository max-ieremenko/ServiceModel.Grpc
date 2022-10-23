using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service;
using Service.Filters;

namespace ServerSelfHost;

public static class Program
{
    public static async Task Main()
    {
        var server = new Server
        {
            Ports = { new ServerPort("localhost", 8081, ServerCredentials.Insecure) }
        };

        using (var serviceProvider = BuildServiceProvider())
        {
            // host Calculator
            server.Services.AddServiceModel<Calculator>(
                serviceProvider,
                options =>
                {
                    options.Filters.Add(1, provider => provider.GetRequiredService<LoggingServerFilter>());
                });

            server.Start();

            try
            {
                await ClientCalls.CallCalculator(new Uri("http://localhost:8081"), CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                await server.ShutdownAsync().ConfigureAwait(false);
            }
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddTransient<Calculator>();

        services.AddTransient<LoggingServerFilter>();

        services.AddLogging(configure => configure.AddConsole());

        return services.BuildServiceProvider();
    }
}