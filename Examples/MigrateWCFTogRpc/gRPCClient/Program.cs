using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace GrpcClient;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory();

    public static async Task Main()
    {
        var channel = GrpcChannel.ForAddress($"http://localhost:{SharedConfiguration.GrpcServicePort}");
        var proxy = DefaultClientFactory.CreateClient<IPersonService>(channel);

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
        Console.WriteLine("Grpc Get person by id = 1");

        var person = await proxy.Get(1);
        Console.WriteLine("  {0}", person);

        Console.WriteLine("Grpc Get person by id = 0");

        person = await proxy.Get(0);
        Trace.Assert(person == null);
        Console.WriteLine("  person not found, id=0");
    }

    private static async Task CallGetAll(IPersonService proxy)
    {
        Console.WriteLine("Grpc Get all persons");

        var persons = await proxy.GetAll();
        foreach (var person in persons)
        {
            Console.WriteLine("  {0}", person);
        }
    }
}