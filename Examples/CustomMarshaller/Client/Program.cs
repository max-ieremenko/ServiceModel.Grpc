using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using CustomMarshaller;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set JsonMarshallerFactory as default Marshaller
            MarshallerFactory = JsonMarshallerFactory.Default
        });

        var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var personService = clientFactory.CreateClient<IPersonService>(channel);

        Console.WriteLine("Invoke CreatePerson");

        var person = await personService.CreatePerson("John X", DateTime.Today.AddYears(-20));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }
}