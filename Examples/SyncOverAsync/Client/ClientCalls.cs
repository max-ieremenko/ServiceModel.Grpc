using System;
using System.Threading.Tasks;
using Contract;
using Grpc.Core;
using ServiceModel.Grpc.Client;

namespace Client;

public sealed class ClientCalls
{
    private readonly IClientFactory _clientFactory;
    private readonly Channel _channel;

    public ClientCalls(int serverPort)
    {
        _clientFactory = new ClientFactory();
        _channel = new Channel("localhost", serverPort, ChannelCredentials.Insecure);
    }

    public void RunSync()
    {
        var personService = _clientFactory.CreateClient<IPersonService>(_channel);

        Console.WriteLine("Invoke Create");

        var person = personService.Create("John X", DateTime.Today.AddYears(-20));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);

        Console.WriteLine("Invoke Update");

        person = personService.Update(person, "John", DateTime.Today.AddYears(-21));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);
    }

    public async Task RunAsync()
    {
        var personService = _clientFactory.CreateClient<IPersonService>(_channel);

        Console.WriteLine("Invoke CreateAsync");

        var person = await personService.CreateAsync("John X", DateTime.Today.AddYears(-20));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);

        Console.WriteLine("Invoke UpdateAsync");

        person = await personService.UpdateAsync(person, "John", DateTime.Today.AddYears(-21), default);

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);
    }
}