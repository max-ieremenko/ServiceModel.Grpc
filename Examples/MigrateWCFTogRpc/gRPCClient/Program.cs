using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace gRPCClient;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory();

    public static async Task Main()
    {
        var aspNetCoreChannel = new Channel("localhost", SharedConfiguration.AspNetgRPCPersonServicePort, ChannelCredentials.Insecure);
        var proxy = DefaultClientFactory.CreateClient<IPersonService>(aspNetCoreChannel);

        Console.WriteLine("-- call AspNetServiceHost --");
        await CallGet(proxy);
        await CallGetAll(proxy);

        var nativeChannel = new Channel("localhost", SharedConfiguration.NativegRPCPersonServicePort, ChannelCredentials.Insecure);
        proxy = DefaultClientFactory.CreateClient<IPersonService>(nativeChannel);

        Console.WriteLine();
        Console.WriteLine("-- call NativeServiceHost --");
        await CallGet(proxy);
        await CallGetAll(proxy);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task CallGet(IPersonService proxy)
    {
        Console.WriteLine("gRPC Get person by id = 1");

        var person = await proxy.Get(1);
        Console.WriteLine("  {0}", person);

        Console.WriteLine("WCF Get person by id = 0");

        person = await proxy.Get(0);
        Trace.Assert(person == null);
        Console.WriteLine("  person not found, id=0");
    }

    private static async Task CallGetAll(IPersonService proxy)
    {
        Console.WriteLine("gRPC Get all persons");

        var persons = await proxy.GetAll();
        foreach (var person in persons)
        {
            Console.WriteLine("  {0}", person);
        }
    }
}