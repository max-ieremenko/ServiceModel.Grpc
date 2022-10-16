﻿using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace gRPCClient
{
    public static class Program
    {
        private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // register client error handler
            ErrorHandler = new FaultExceptionClientHandler()
        });

        public static async Task Main()
        {
            var aspNetCoreChannel = new Channel("localhost", SharedConfiguration.AspNetgRPCDebugServicePort, ChannelCredentials.Insecure);
            var proxy = DefaultClientFactory.CreateClient<IDebugService>(aspNetCoreChannel);

            Console.WriteLine("-- call AspNetServiceHost --");
            await CallThrowApplicationException(proxy);
            await CallThrowInvalidOperationException(proxy);

            var nativeChannel = new Channel("localhost", SharedConfiguration.NativegRPCDebugServicePort, ChannelCredentials.Insecure);
            proxy = DefaultClientFactory.CreateClient<IDebugService>(nativeChannel);

            Console.WriteLine();
            Console.WriteLine("-- call NativeServiceHost --");
            await CallThrowApplicationException(proxy);
            await CallThrowInvalidOperationException(proxy);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("...");
                Console.ReadLine();
            }
        }

        private static async Task CallThrowApplicationException(IDebugService proxy)
        {
            Console.WriteLine("gRPC call ThrowApplicationException");

            try
            {
                await proxy.ThrowApplicationException("some message");
            }
            catch (FaultException<ApplicationExceptionFaultDetail> ex)
            {
                Console.WriteLine("  Error message: {0}", ex.Detail.Message);
            }
        }

        private static async Task CallThrowInvalidOperationException(IDebugService proxy)
        {
            Console.WriteLine("gRPC call ThrowApplicationException");

            try
            {
                await proxy.ThrowInvalidOperationException("some message");
            }
            catch (FaultException<InvalidOperationExceptionFaultDetail> ex)
            {
                Console.WriteLine("  Error message: {0}", ex.Detail.Message);
                Console.WriteLine("  StackTrace: {0}", ex.Detail.StackTrace);
            }
        }
    }
}
