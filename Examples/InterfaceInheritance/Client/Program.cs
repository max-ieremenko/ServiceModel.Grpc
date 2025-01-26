using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var clientFactory = new ClientFactory();

        // register generated IGenericCalculator<int> proxy, see MyGrpcProxies
        clientFactory.AddGenericCalculatorInt32Client();

        var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions());

        // create instance of GenericCalculatorInt32Client, see MyGrpcProxies
        var genericCalculator = clientFactory.CreateClient<IGenericCalculator<int>>(channel);
        await InvokeGenericCalculator(genericCalculator);

        // proxy will be generated on-fly
        var doubleCalculator = clientFactory.CreateClient<IDoubleCalculator>(channel);
        await InvokeDoubleCalculator(doubleCalculator);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static async Task InvokeGenericCalculator(IGenericCalculator<int> proxy)
    {
        // POST /IGenericCalculator-Int32/Touch
        Console.WriteLine("Invoke Touch");
        var touchResponse = proxy.Touch();
        Console.WriteLine("  {0}", touchResponse);

        // POST /IGenericCalculator-Int32/GetRandomValue
        Console.WriteLine("Invoke GetRandomValue");
        var x = await proxy.GetRandomValue();
        var y = await proxy.GetRandomValue();
        Console.WriteLine("  X = {0}", x);
        Console.WriteLine("  Y = {0}", y);

        // POST /IGenericCalculator-Int32/Sum
        Console.WriteLine("Invoke Sum");
        var sumResponse = await proxy.Sum(x, y);
        Console.WriteLine("  {0} + {1} = {2}", x, y, sumResponse);

        // POST /IGenericCalculator-Int32/Multiply
        Console.WriteLine("Invoke Multiply");
        var multiplyResponse = await proxy.Multiply(x, y);
        Console.WriteLine("  {0} * {1} = {2}", x, y, multiplyResponse);
    }

    private static async Task InvokeDoubleCalculator(IDoubleCalculator proxy)
    {
        // POST /IDoubleCalculator/Touch
        Console.WriteLine("Invoke Touch");
        var touchResponse = proxy.Touch();
        Console.WriteLine("  {0}", touchResponse);

        // POST /IDoubleCalculator/GetRandomValue
        Console.WriteLine("Invoke GetRandomValue");
        var x = await proxy.GetRandomValue();
        var y = await proxy.GetRandomValue();
        Console.WriteLine("  X = {0}", x);
        Console.WriteLine("  Y = {0}", y);

        // POST /IDoubleCalculator/Sum
        Console.WriteLine("Invoke Sum");
        var sumResponse = await proxy.Sum(x, y);
        Console.WriteLine("  {0} + {1} = {2}", x, y, sumResponse);

        // POST /IDoubleCalculator/Multiply
        Console.WriteLine("Invoke Multiply");
        var multiplyResponse = await proxy.Multiply(x, y);
        Console.WriteLine("  {0} * {1} = {2}", x, y, multiplyResponse);
    }
}