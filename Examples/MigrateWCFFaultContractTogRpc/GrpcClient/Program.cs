using System;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace GrpcClient;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // register client error handler
        ErrorHandler = new FaultExceptionClientHandler()
    });

    public static async Task Main()
    {
        var channel = GrpcChannel.ForAddress($"http://localhost:{SharedConfiguration.GrpcServicePort}");
        var proxy = DefaultClientFactory.CreateClient<IDebugService>(channel);

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
        Console.WriteLine("Grpc call ThrowApplicationException");

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
        Console.WriteLine("Grpc call ThrowApplicationException");

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