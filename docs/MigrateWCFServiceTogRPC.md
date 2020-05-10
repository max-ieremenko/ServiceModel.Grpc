# Migrate a WCF service and client to a gRPC with ServiceModel.Grpc

This tutorial shows how to migrate existing WCF service and client to gRPC with minimum effort.

[View sample code](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc).

## The existing WCF solution

The [MigrateWCFTogRpc.sln](https://github.com/max-ieremenko/ServiceModel.Grpc/blob/master/Examples/MigrateWCFTogRpc) includes a simple request-response Person service. The service is defined in the interface IPersonService as WCF service contract:

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

The Person model is a simple data contract class:

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
* [WCFServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/WCFServiceHost) - net461, hosts WCF endpoint "http://localhost:8000/PersonService.svc"
* [WCFClient](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/WCFClient) - net461, makes WCF calls to endpoint "http://localhost:8000/PersonService.svc"

The PersonService implementation uses a repository class provided via unity dependency injection.

## Migrate to gRPC

With ServiceModel.Grpc migration is simple:

* no changes in `Contract` and `Service`
* no .proto files
* on server-side only hosting has to be changed
* on client-side only WCF ChannelFactory has to be replaced by `ServiceModel.Grpc.Client.ClientFactory`

## Host PersonService in ASP.NET Core server

ASP.NET Core server hosting requires netcoreapp3.0 or higher.

Tutorial how to create and configure ASP.NET Core server project is [here](CreateClientAndServerASPNETCore.md).

Project [AspNetServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/AspNetServiceHost) is already configured and has reference to nuget package [ServiceModel.Grpc.AspNetCore](https://www.nuget.org/packages/ServiceModel.Grpc.AspNetCore/) which provides code-first approach for Grpc.AspNetCore.Server.

All required configuration to host PersonService is done in the Startup.cs:

``` c#
internal sealed class Startup
{
    public void ConfigureContainer(IUnityContainer container)
    {
        // configure container
        PersonModule.ConfigureContainer(container);
    }

    public void ConfigureServices(IServiceCollection services)
    {
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

Grpc.Core server hosting is available for net461 and netcoreapp.

Tutorial how to create and configure Grpc.Core server project is [here](GrpcCoreServerConfiguration.md).

Project [NativeServiceHost](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/NativeServiceHost) is already configured and has reference to nuget package [ServiceModel.Grpc.SelfHost](https://www.nuget.org/packages/ServiceModel.Grpc.SelfHost/) which provides code-first approach for Grpc.Core.Server.

All required configuration to host PersonService is done in the Program.cs:

``` c#
// create and configure container
var container = new UnityContainer();
PersonModule.ConfigureContainer(container);

// create and configure Grpc.Core.Server
var server = new Server { /*  */ };

// host PersonService provided by UnityContainer
server.Services.AddServiceModelTransient(container.Resolve<Func<PersonService>>());
```

## Migrate client

Project [gRPCClient](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples/MigrateWCFTogRpc/gRPCClient) has reference to nuget package [ServiceModel.Grpc](https://www.nuget.org/packages/ServiceModel.Grpc/) which provides code-first approach for [Grpc.Core](https://www.nuget.org/packages/Grpc.Core).

``` c#
// create ClientFactory
private static readonly IClientFactory DefaultClientFactory = new ClientFactory();

// create gRPC Channel
var channel = new Channel("localhost", 8080, ChannelCredentials.Insecure);

// create client
var proxy = DefaultClientFactory.CreateClient<IPersonService>(aspNetCoreChannel);
```

## Summary

In order to migrate WCF service and client to gRPC with ServiceModel.Grpc only the service hosting has to changed and the way how to create a proxy.

## What is next

Migrate FaultContract exception handling to a gRPC global [error handling](migrate-wcf-faultcontract-to-global-error-handling.md).