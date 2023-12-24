using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var configuration = new ConfigurationManager()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        await Run(Client.ReflectionEmit.DemoGrpcNetChannel.ConfigureServices1, configuration);
        await Run(Client.ReflectionEmit.DemoGrpcNetChannel.ConfigureServices2, configuration);
        await Run(Client.ReflectionEmit.DemoGrpcNetChannel.ConfigureServices3, configuration);
        await Run(Client.ReflectionEmit.DemoGrpcNetClientFactory.ConfigureServices, configuration);

        await Run(Client.DesignTime.DemoGrpcNetChannel.ConfigureServices1, configuration);
        await Run(Client.DesignTime.DemoGrpcNetChannel.ConfigureServices2, configuration);
        await Run(Client.DesignTime.DemoGrpcNetChannel.ConfigureServices3, configuration);
        await Run(Client.DesignTime.DemoGrpcNetClientFactory.ConfigureServices, configuration);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task Run(Action<IServiceCollection> configureServices, IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole());
        services.Configure<ClientConfiguration>(configuration.GetSection(nameof(ClientConfiguration)));
        services.AddTransient<Worker>();

        configureServices(services);

        await using var serviceProvider = services.BuildServiceProvider();

        var worker = serviceProvider.GetRequiredService<Worker>();
        worker.Logger.LogInformation($"--- {configureServices.Method.DeclaringType?.Name}.{configureServices.Method.Name} ---");
        await worker.Run();
    }
}