# ServiceModel.Grpc Grpc.Core server configuration

## AddService...

```C#
var server = new Grpc.Core.Server();

server.Services.AddServiceModelSingleton(
    new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory  = ...
    });

server.Services.AddServiceModelTransient(
    () => new MyService(),
    options =>
    {
        // service configuration
        options.MarshallerFactory  = ...
    });
```

#### ServiceModelGrpcServiceOptions

- IMarshallerFactory MarshallerFactory: by default is null, it means DataContractMarshallerFactory.Default
- ILogger Logger: by default is null. To setup possible output provided by service binding

## Silent proxy generation

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
