# ServiceModel.Grpc Create a gRPC client and server in ASP.NET Core

This tutorial shows how to create a .NET client and an ASP.NET Core Server.
At the end, you'll have a gRPC client that communicates with the gRPC Greeter service.

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/CreateClientAndServerASPNETCore).

## Create contract first

Create a blank solution. Add new "Class Library (.NET standard)" project with name "Contract".

#### Add required packages

- [System.ServiceModel.Primitives](https://www.nuget.org/packages/System.ServiceModel.Primitives/), which contains ServiceContractAttribute and OperationContractAttribute

#### Add class Person

Person is decorated for data contract [serialization](https://docs.microsoft.com/en-us/dotnet/framework/wcf/samples/datacontractserializer-sample).

``` c#
using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class Person
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string SecondName { get; set; }
    }
}
```

#### Add service contract

Contract is decorated with ServiceContractAttribute and OperationContractAttribute.

``` c#
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IGreeter
    {
        [OperationContract]
        Task<string> SayHelloAsync(string personFirstName, string personSecondName, CancellationToken token = default);

        [OperationContract]
        Task<string> SayHelloToAsync(Person person, CancellationToken token = default);
    }
}
```

## Create service

Add new "ASP.NET Core Web Application" project with name "Service".
In the wizard select "Empty (an empty project template...)" and un-check "Configure for HTTPS".

#### Add required references

- add reference to the "Contract" project
- add reference to [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore/)

#### Add Greeter service

``` c#
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service
{
    internal sealed class Greeter : IGreeter
    {
        public Task<string> SayHelloAsync(string personFirstName, string personSecondName, CancellationToken token)
        {
            return Task.FromResult(string.Format("Hello {0} {1}", personFirstName, personSecondName));
        }

        public Task<string> SayHelloToAsync(Person person, CancellationToken token)
        {
            return Task.FromResult(string.Format("Hello {0} {1}", person.FirstName, person.SecondName));
        }
    }
}
```

#### Configure ServiceModel.Grpc

In the Startup.cs, ConfigureServices method register ServiceModel.Grpc

``` c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // register ServiceModel.Grpc
        services.AddServiceModelGrpc();
    }

    // ....
}
```

#### Bind Greeter

In the Startup.cs, Configure method bind Greeter service

``` c#
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // bind Greeter service
            endpoints.MapGrpcService<Greeter>();
        });
    }
}
```

#### Configure http2

http2 is a precondition for gRPC commination [protocol](https://github.com/grpc/grpc/blob/master/doc/PROTOCOL-HTTP2.md).

In the Program.cs, method CreateHostBuilder add kestrel configuration

``` c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseKestrel(options =>
            {
                // set http2 protocol
                options.ConfigureEndpointDefaults(endpoints => endpoints.Protocols = HttpProtocols.Http2);
            });
        });
```

In the "Service" project properties -> Debug tab -> Profile select "Service".
By default "IIS Express" is selected.

## Create client

Add new "Console App (.NET Core)" project with name "Client".

#### Add required references

- add reference to the "Contract" project
- add reference to [Grpc.Core](https://www.nuget.org/packages/Grpc.Core/)
- add reference to [ServiceModel.Grpc](https://www.nuget.org/packages/ServiceModel.Grpc/)

#### Make application entry point async

``` c#
public static class Program
{
    public static async Task Main(string[] args)
    {
    }
}
```

#### Configure ServiceModel.Grpc client factory

In the Program.cs, create static link to ServiceModel.Grpc.Client.ClientFactory

``` c#
public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory();
}
```

#### Create gRPC channel

To create a channel you have to know a port of "Service" application.
You can find or change it in the Service\Properties\launchSettings.json.

By default when you run Service application the url from "Service" is used.
In this example the port is 5000.

``` c#
var channel = new Channel("localhost", 5000, ChannelCredentials.Insecure);
```

#### Create client calls

``` c#
public static class Program
{
    private static readonly IClientFactory DefaultClientFactory = new ClientFactory();

    public static async Task Main(string[] args)
    {
        // create gRPC channel
        var channel = new Channel("localhost", 50516, ChannelCredentials.Insecure);

        // create IGreeter client proxy
        var client = DefaultClientFactory.CreateClient<IGreeter>(channel);

        var person = new Person { FirstName = "John", SecondName = "X" };

        var greet1 = await client.SayHelloAsync(person.FirstName, person.SecondName);
        Console.WriteLine(greet1);

        var greet2 = await client.SayHelloToAsync(person);
        Console.WriteLine(greet2);
   }
}
```

## Run application

Run "Service" and then "Client"

## What is next

Configure custom [error handling](global-error-handling.md).