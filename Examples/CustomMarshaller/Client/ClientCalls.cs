using System;
using System.Threading.Tasks;
using Contract;
using CustomMarshaller;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace Client;

public sealed class ClientCalls
{
    public static async Task Run(int serverPort)
    {
        var clientFactory = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            // set JsonMarshallerFactory as default Marshaller
            MarshallerFactory = JsonMarshallerFactory.Default
        });

        var channel = new Channel("localhost", serverPort, ChannelCredentials.Insecure);
        var personService = clientFactory.CreateClient<IPersonService>(channel);

        Console.WriteLine("Invoke CreatePerson");

        var person = await personService.CreatePerson("John X", DateTime.Today.AddYears(-20));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);
    }
}