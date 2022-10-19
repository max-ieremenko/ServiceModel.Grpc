using System;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service;

namespace ServerSelfHost
{
    public static class Program
    {
        public static void Main()
        {
            var server = new Server
            {
                Ports = { new ServerPort("localhost", 5000, ServerCredentials.Insecure) }
            };

            using (var serviceProvider = BuildServiceProvider())
            {
                // host GreeterService
                server.Services.AddServiceModel<GreeterService>(
                    serviceProvider,
                    options =>
                    {
                        options.ConfigureServiceDefinition = definition =>
                        {
                            var interceptor = options.ServiceProvider!.GetRequiredService<ServerLoggerInterceptor>();
                            return definition.Intercept(interceptor);
                        };
                    });

                server.Start();

                var cancellationSource = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    cancellationSource.Cancel();
                };

                Console.WriteLine("Press CTRL+C for exit...");
                cancellationSource.Token.WaitHandle.WaitOne();
            }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<GreeterService>();
            services.AddTransient<ServerLoggerInterceptor>();

            services.AddLogging(configure => configure.AddConsole());

            return services.BuildServiceProvider();
        }
    }
}
