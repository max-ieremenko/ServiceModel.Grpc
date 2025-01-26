using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contract;
using Grpc.Net.Client;
using ServiceModel.Grpc.Client;

namespace Client;

public static class Program
{
    public static async Task Main()
    {
        var clientFactory = new ClientFactory();
        var channel = GrpcChannel.ForAddress("http://localhost:5000");

        var personService = clientFactory.CreateClient<IPersonService>(channel);

        RunSync(personService);
        await RunAsync(personService);

        if (Debugger.IsAttached)
        {
            Console.WriteLine("...");
            Console.ReadLine();
        }
    }

    private static void RunSync(IPersonService personService)
    {
        Console.WriteLine("Invoke Create");

        var person = personService.Create("John X", DateTime.Today.AddYears(-20));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);

        Console.WriteLine("Invoke Update");

        person = personService.Update(person, "John", DateTime.Today.AddYears(-21));

        Console.WriteLine("  Name: {0}", person.Name);
        Console.WriteLine("  BirthDay: {0}", person.BirthDay);
    }

    private static async Task RunAsync(IPersonService personService)
    {
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