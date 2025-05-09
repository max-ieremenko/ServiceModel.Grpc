﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Services;

namespace Server;

internal sealed class ServerHost : IHostedService
{
    private const int Port = 8082;

    private readonly Grpc.Core.Server _server;

    public ServerHost(IServiceProvider serviceProvider)
    {
        _server = new()
        {
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };

        // host Calculator service, gRPC endpoint will be generated at runtime by ServiceModel.Grpc
        // see https://max-ieremenko.github.io/ServiceModel.Grpc/GrpcCoreServerConfiguration.html
        _server.Services.AddServiceModel<Calculator>(
            serviceProvider,
            options =>
            {
                // the DataContractMarshallerFactory is default one
                //options.MarshallerFactory = DataContractMarshallerFactory.Default;

                // add server filters, see https://max-ieremenko.github.io/ServiceModel.Grpc/server-filters.html
                //options.Filters.Add(...);

                // configure error handling, see https://max-ieremenko.github.io/ServiceModel.Grpc/error-handling-general.html
                //options.ErrorHandler = ...

                // optionally, attach an Interceptor
                options.ConfigureServiceDefinition = definition =>
                {
                    var interceptor = options.ServiceProvider!.GetRequiredService<ServerLoggerInterceptor>();
                    return definition.Intercept(interceptor);
                };
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}