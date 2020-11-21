# ServiceModel.Grpc client configuration

## ClientFactory

ClientFactory does not have any static methods as usually default configuration is good only for "hello world" project.

``` c#
public static class Program
{
    private static readonly IClientFactory DefaultClientFactory
        = new ClientFactory(new ServiceModelGrpcClientOptions
        {
            MarshallerFactory = ...
            DefaultCallOptionsFactory = () => ...
            ErrorHandler = ...
        });

    public static async Task Main(string[] args) { }
}
```

#### ServiceModelGrpcClientOptions:

- IMarshallerFactory MarshallerFactory: by default is null (DataContractMarshallerFactory.Default will be used)
- Func\<CallOptions\> DefaultCallOptionsFactory: by default is null. Allows to setup default CallOptions for all calls by all clients created by this ClientFactory
- IClientErrorHandler ErrorHandler: by default is null (error handling by gRPC API)
- ILogger Logger: by default is null. Allows to catch possible output provided by this ClientFactory

#### IClientFactory.AddClient\<TContract\>(Action\<ServiceModelGrpcClientOptions\>?)

The method generates a proxy for `IMyContract` via Reflection.Emit and applies the configuration for proxy.

``` c#
DefaultClientFactory.AddClient<IMyContract>(options =>
{
    // setup ServiceModelGrpcClientOptions for this client
    // by default options contain values from default factory configuration
});
```

#### IClientFactory.AddClient\<TContract\>(IClientBuilder\<TContract\>, Action\<ServiceModelGrpcClientOptions\>?)

The method registers a specific proxy builder for `IMyContract` and applies the configuration for proxy. The method is used by source code generator.

Configure ServiceModel.Grpc.DesignTime to generate a source code of `IMyContract` proxy:

``` c#
[ImportGrpcService(typeof(IMyContract))]
internal static partial class MyGrpcServices
{
    // generated code ...
    public static IClientFactory AddMyContractClient(this IClientFactory clientFactory, Action<ServiceModelGrpcClientOptions> configure = null) {}
}
```

register generated proxy `IMyContract`:

``` c#
DefaultClientFactory.AddMyContractClient(options =>
{
    // setup ServiceModelGrpcClientOptions for this client
    // by default options contain values from default factory configuration
});
```

#### IClientFactory.CreateClient\<TContract\>()

`CreateClient` creates an new instance of a specific client with previously assigned configuration.

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

var client = DefaultClientFactory.CreateClient<IContract>()
```

the proxy will be created by the factory without any errors, but at runtime a method invocation will throw NotSupportedException:

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

``` c#
var factory = new ClientFactory();
for (var i=0; i<10; i++)
{
    factory.CreateClient<IContract>()
}
```
