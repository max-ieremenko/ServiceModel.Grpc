using System;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;
using ServiceModel.Grpc.Configuration;

namespace Client;

public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        // set MessagePackMarshaller as default Marshaller
        MarshallerFactory = MessagePackMarshallerFactory.Default
    });

    public static async Task Main()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:8080");
        var personService = DefaultClientFactory.CreateClient<IPersonService>(channel);

        var person = await personService.CreatePerson("John X", DateTime.Today.AddYears(-20));
        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);
        Console.WriteLine("  CreatedBy: {0}", person.CreatedBy);
    }
}