﻿using Grpc.Core;
using Microsoft.Extensions.Hosting;
using ServiceModel.Grpc.Interceptors;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Core.Logging;
using Service.Shared;

namespace ServerNativeHost;

internal sealed class ServerHost : IHostedService
{
    private readonly Server _server;
    
    public ServerHost()
    {
        _server = new Server
        {
            Ports =
            {
                new ServerPort("localhost", ServiceConfiguration.ServiceNativeGrpcPort, ServerCredentials.Insecure)
            }
        };

        _server.Services.AddServiceModelSingleton(
            new DebugService(),
            options =>
            {
                // enable ServiceModel.Grpc logging
                options.Logger = new ConsoleLogger();

                // combine application and unexpected handlers into one handler
                options.ErrorHandler = new ServerErrorHandlerCollection(
                    new ApplicationExceptionServerHandler(),
                    new UnexpectedExceptionServerHandler());

                // uncomment to fully control ServerFaultDetail.Detail serialization, must be uncommented in Client as well
                //options.ErrorDetailSerializer = new CustomServerFaultDetailSerializer();
            });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => _server.ShutdownAsync();
}