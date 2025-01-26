using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using Contract;

namespace WcfClient;

public static class Program
{
    public static async Task Main()
    {
        using (var factory = new ChannelFactory<IDebugService>(new BasicHttpBinding(), SharedConfiguration.WcfDebugServiceLocation))
        {
            var proxy = factory.CreateChannel();
            await CallThrowApplicationException(proxy);
            await CallThrowInvalidOperationException(proxy);
        }

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task CallThrowApplicationException(IDebugService proxy)
    {
        Console.WriteLine("WCF call ThrowApplicationException");

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
        Console.WriteLine("WCF call ThrowInvalidOperationException");

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