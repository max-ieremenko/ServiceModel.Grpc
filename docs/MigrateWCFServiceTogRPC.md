# Migrate a WCF service and client to a gRPC with ServiceModel.Grpc

This page shows how to migrate existing WCF services and clients to gRPC with minimum effort.

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc).

## The existing WCF solution

The [MigrateWCFTogRpc.sln](https://github.com/max-ieremenko/ServiceModel.Grpc/blob/master/Examples/MigrateWCFTogRpc) includes a simple request-response Person service. WCF service contract:

``` c#
[ServiceContract]
public interface IPersonService
{
    [OperationContract]
    Task<Person> Get(int personId);

    [OperationContract]
    Task<IList<Person>> GetAll();
}
```

The Person type is a simple data contract class:

``` c#
[DataContract]
public class Person
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string FirstName { get; set; }

    [DataMember]
    public string SecondName { get; set; }
}
```

Projects

* [Contract](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/Contract) - netstandard2.0, contains the service contract and data contract
* [Service](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/Service) - netstandard2.0, contains the person service implementation (business logic)
* [WCFServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/WCFServiceHost) - net462, hosts WCF endpoint "http://localhost:8000/PersonService.svc"
* [WCFClient](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/WCFClient) - net462, makes WCF calls to endpoint "http://localhost:8000/PersonService.svc"

## Migrate to gRPC

With ServiceModel.Grpc migration is simple:

* no changes in `Contract` and `Service`
* no .proto files
* on the server-side only hosting has to be changed
* on the client-side only WCF ChannelFactory has to be replaced by `ServiceModel.Grpc.Client.ClientFactory`

## Host PersonService in ASP.NET Core server

The project [AspNetServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/AspNetServiceHost) is already configured and has reference to the nuget package [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore/).

All required configuration to host PersonService is done in the Startup.cs:

``` c#
internal sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // configure service provider
        PersonModule.ConfigureContainer(services);

        // enable ServiceModel.Grpc
        services.AddServiceModelGrpc();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // host PersonService
            endpoints.MapGrpcService<PersonService>();
        });
    }
}
```

## Host PersonService in Grpc.Core server

The project [NativeServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/NativeServiceHost) is already configured and has reference to the nuget package [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost/).

All required configuration to host PersonService is done in Program.cs and ServerHost.cs:

``` c#
// create service provider
PersonModule.ConfigureServices(services);

// create and configure Grpc.Core.Server
_server = new Server { /*  */ };

// host PersonService
_server.Services.AddServiceModel<PersonService>(serviceProvider);
```

## Migrate client

The project [gRPCClient](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/gRPCClient) has reference to nuget package [ServiceModel.Grpc](https://www.nuget.org/packages/ServiceModel.Grpc/).

``` c#
// create ClientFactory
private static readonly IClientFactory DefaultClientFactory = new ClientFactory();

// create gRPC Channel
var channel = new Channel("localhost", 8080, ChannelCredentials.Insecure);

// create client
var proxy = DefaultClientFactory.CreateClient<IPersonService>(aspNetCoreChannel);
```

## What is next

Migrate FaultContract exception handling to a gRPC global [error handling](migrate-wcf-faultcontract-to-global-error-handling.md).