using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Client.Services;
using Contract;
using Grpc.Core;
using Grpc.Net.Client;
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
        ErrorHandler = new ClientErrorHandlerCollection(new ApplicationExceptionClientHandler(), new UnexpectedExceptionClientHandler()),

        // uncomment to fully control ClientFaultDetail.Detail deserialization, must be uncommented in Server as well
        //ErrorDetailDeserializer = new CustomClientFaultDetailDeserializer()
    });

    public static async Task Main()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5000");

        Console.WriteLine();
        Console.WriteLine($"Invoke {nameof(ThrowApplicationException)}");
        await ThrowApplicationException(channel);

        Console.WriteLine();
        Console.WriteLine($"Invoke {nameof(ThrowRandomException)}");
        await ThrowRandomException(channel);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task ThrowApplicationException(ChannelBase channel)
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

    private static async Task ThrowRandomException(ChannelBase channel)
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