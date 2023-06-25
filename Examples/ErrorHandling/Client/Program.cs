using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Interceptors;

namespace Client;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // enable ServiceModel.Grpc logging
        Logger = new SimpleConsoleLogger(),

        // combine application and unexpected handlers into one handler
        ErrorHandler = new ClientErrorHandlerCollection(new ApplicationExceptionClientHandler(), new UnexpectedExceptionClientHandler())
    });

    public static async Task Main()
    {
        var nativeChannel = new Channel("localhost", ServiceConfiguration.ServiceNativeGrpcPort, ChannelCredentials.Insecure);
        var serviceModelChannel = new Channel("localhost", ServiceConfiguration.ServiceModelGrpcPort, ChannelCredentials.Insecure);

        Console.WriteLine();
        Console.WriteLine("Invoke ThrowApplicationException on ServerAspNetHost");
        await InvokeThrowApplicationException(serviceModelChannel);

        Console.WriteLine();
        Console.WriteLine("Invoke ThrowApplicationException on ServerNativeHost");
        await InvokeThrowApplicationException(nativeChannel);

        Console.WriteLine();
        Console.WriteLine("Invoke ThrowRandomException on ServerAspNetHost");
        await InvokeThrowRandomException(serviceModelChannel);

        Console.WriteLine();
        Console.WriteLine("Invoke ThrowRandomException on ServerNativeHost");
        await InvokeThrowRandomException(nativeChannel);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task InvokeThrowApplicationException(ChannelBase channel)
    {
        var client = DefaultClientFactory.CreateClient<IDebugService>(channel);

        try
        {
            await client.ThrowApplicationException("  application error occur");
        }
        catch (ApplicationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task InvokeThrowRandomException(ChannelBase channel)
    {
        var client = DefaultClientFactory.CreateClient<IDebugService>(channel);

        try
        {
            await client.ThrowRandomException("random error occur");
        }
        catch (UnexpectedErrorException ex)
        {
            Console.WriteLine("  Message: {0}", ex.Detail.Message);
            Console.WriteLine("  ExceptionType: {0}", ex.Detail.ExceptionType);
            Console.WriteLine("  MethodName: {0}", ex.Detail.MethodName);
            Console.WriteLine("  FullException: {0} ...", new StringReader(ex.Detail.FullException).ReadLine());
        }
    }
}