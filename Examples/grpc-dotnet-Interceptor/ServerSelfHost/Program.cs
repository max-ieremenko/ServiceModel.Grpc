using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service;

namespace ServerSelfHost
{
    public static class Program
    {
        public static void Main(string[] args)
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

                Console.WriteLine("Press enter to exit . . .");
                Console.ReadLine();
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
