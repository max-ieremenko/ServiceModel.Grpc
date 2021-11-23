# ServiceModel.Grpc Grpc.Core server configuration

## AddService...

``` c#
var server = new Grpc.Core.Server();

server.Services.AddServiceModelSingleton(
    new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory  = ...
        options.ErrorHandler  = ...
        options.ServiceProvider  = ...
        options.Filters ...
    });

server.Services.AddServiceModelTransient(
    () => new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory  = ...
        options.ErrorHandler  = ...
        options.ServiceProvider  = ...
        options.Filters ...
    });

// register MyService in serviceProvider
IServiceProvider serviceProvider = ...;

server.Services.AddServiceModel<MyService>(
    serviceProvider,
    options =>
    {
        // service configuration
        options.MarshallerFactory  = ...
        options.ErrorHandler  = ...
        options.ServiceProvider  = ...
        options.Filters ...
    });
```

#### ServiceModelGrpcServiceOptions

- IMarshallerFactory MarshallerFactory: by default is null (DataContractMarshallerFactory.Default)
- IServerErrorHandler ErrorHandler; by default is null (error handling by gRPC API)
- ILogger Logger: by default is null. To setup possible output provided by service binding
- IServiceProvider ServiceProvider: service provider instance
- FilterCollection\<IServerFilter\> Filters: collection of server filters

## Silent proxy generation (Reflection.Emit)

In this example

``` c#
[ServiceContract]
public interface IContract
{
    [OperationContract]
    Task<T> SumAsync<T>(T x, T y);

    void Dispose();
}

internal sealed class Service : IContract { }
```

SumAsync and Dispose will be ignored during the service binding, use ServiceModelGrpcServiceOptions.ILogger to see warnings.
