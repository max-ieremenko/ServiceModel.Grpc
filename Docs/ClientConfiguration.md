# ServiceModel.Grpc client configuration

## ClientFactory

ClientFactory does not have any static methods, because usually default configuration is good only for "hello world" project.

``` c#
public static class Program
{
    private static readonly IClientFactory DefaultClientFactory
        = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            MarshallerFactory = ...
            DefaultCallOptionsFactory = () => ...
        });

    public static async Task Main(string[] args) { }
}
```

#### ServiceModelGrpcClientOptions:

- IMarshallerFactory MarshallerFactory: by default is null, it means DataContractMarshallerFactory.Default
- Func\<CallOptions\> DefaultCallOptionsFactory: by default is null. To setup default CallOptions for all calls by all clients created by this ClientFactory
- ILogger Logger: by default is null. To setup possible output provided by this ClientFactory

#### IClientFactory.AddClient\<TContract\>()

`AddClient` allows to change the configuration for a specific client in this factory.

``` c#
DefaultClientFactory.AddClient<IMyContract>(options =>
{
    // setup ServiceModelGrpcClientOptions for this client
    // by default options contain values from default factory configuration
});
```

#### IClientFactory.CreateClient<TContract>()

`CreateClient` creates an new instance of a specific client with previously assigned configuration.

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

var client = DefaultClientFactory.CreateClient<IContract>()
```

the proxy will be created by the factory without any errors, but at runtime

``` c#
// throw NotSupportedException("... generic methods are not supported ...")
client.SumAsync<int>(1, 3);

// throw NotSupportedException("... method is not operation contract ...")
client.Dispose();
```

Use ServiceModelGrpcClientOptions.Logger to see warnings from ClientFactory.

## One contract, but 2 clients with different configuration

``` c#
[ServiceContract]
public interface IContract { }

var factory1 = new ClientFactory(/* configuration 1*/);
var factory2 = new ClientFactory(/* configuration 2*/);

// client with configuration 1
var client1 = factory1.CreateClient<IContract>();
// client with configuration 2
var client2 = factory2.CreateClient<IContract>();
```

## Make your ClientFactory instance singleton

The following code will create 10 different implementations of IContract under the hood.

``` c#
// bad example
for (var i=0; i<10; i++)
{
    new ClientFactory().CreateClient<IContract>()
}
```

make ClientFactory singleton

``` c#
// good example
var factory = new ClientFactory();
for (var i=0; i<10; i++)
{
    factory.CreateClient<IContract>()
}
```
