using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var clientFactory = CreateClientFactory();
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var calculator = clientFactory.CreateClient<ICalculator>(channel);

        var rectangle = await calculator.CreateRectangleAsync((0, 2), (2, 2), (2, 0), (0, 0));
        Log($"created {rectangle}");

        var area = await calculator.GetAreaAsync(rectangle);
        Log($"rectangle area is {area}");

        var vertices = await calculator.GetVerticesAsync(rectangle);
        Log($"rectangle vertices are {string.Join(", ", (IEnumerable<Point>)vertices)}");
    }

    private static IClientFactory CreateClientFactory()
    {
        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set MemoryPackMarshaller as default Marshaller
            MarshallerFactory = MemoryPackMarshallerFactory.Default
        });

        GrpcClients.AddAllClients(clientFactory);
        return clientFactory;
    }

    private static void Log(string message) => Console.WriteLine($"Client: {message}");
}