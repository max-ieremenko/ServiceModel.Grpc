using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace Client;

public static class ClientCalls
{
    public static async Task RunAsync(int port, CancellationToken cancellationToken = default)
    {
        var clientFactory = CreateClientFactory();
        using var channel = GrpcChannel.ForAddress($"http://localhost:{port}");

        var calculator = clientFactory.CreateClient<ICalculator>(channel);

        var rectangle = await calculator.CreateRectangleAsync((0, 2), (2, 2), (2, 0), (0, 0), cancellationToken);
        Log($"created {rectangle}");

        var area = await calculator.GetAreaAsync(rectangle, cancellationToken);
        Log($"rectangle area is {area}");

        var vertices = await calculator.GetVerticesAsync(rectangle, cancellationToken);
        Log($"rectangle vertices are {string.Join(", ", (IEnumerable<Point>)vertices)}");
    }

    private static IClientFactory CreateClientFactory()
    {
        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set MemoryPackMarshaller as default Marshaller
            MarshallerFactory = MemoryPackMarshallerFactory.Default
        });

        GrpcServices.AddAllClients(clientFactory);
        return clientFactory;
    }

    private static void Log(string message) => Console.WriteLine($"Client: {message}");
}