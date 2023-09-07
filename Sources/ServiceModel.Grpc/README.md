# ServiceModel.Grpc

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc` is package with main functionality, basic Grpc.Core.Api extensions and ClientFactory. 

## Declare a service contract

A service contract is an interface decorated with the `ServiceContract` attribute. Interface methods decorated with the `OperationContract` attribute are treated as gRPC operations.

For example, following `ICalculator` contract with unary `Sum` operation and client streaming `MultiplyBy` operation:

```csharp
[ServiceContract]
public interface ICalculator
{
    [OperationContract]
    Task<long> Sum(long x, int y, int z, CancellationToken token = default);

    [OperationContract]
    ValueTask<(int Multiplier, IAsyncEnumerable<int> Values)> MultiplyBy(IAsyncEnumerable<int> values, int multiplier, CancellationToken token = default);
}
```

## Configure ClientFactory

`ClientFactory` serves to configure and create instances of gRPC service clients.

```csharp
IClientFactory defaultClientFactory
    = new ClientFactory(new ServiceModelGrpcClientOptions
    {
        MarshallerFactory = ...
        DefaultCallOptionsFactory = () => ...
        ErrorHandler = ...
        Logger = ...
        ServiceProvider = ...
        Filters = ...
    });
```

## Configure a gRPC channel

A channel represents a long-lived connection to a gRPC service.

`Grpc.Net.Client.GrpcChannel` from [Grpc.Net.Client](https://www.nuget.org/packages/Grpc.Net.Client) NuGet package.

```csharp
ChannelBase channel = GrpcChannel.ForAddress("http://localhost:5000");
```

`Grpc.Core.Channel` from [Grpc.Core](https://www.nuget.org/packages/Grpc.Core) NuGet package.

```csharp
ChannelBase channel = new Channel("localhost", 5000, ChannelCredentials.Insecure);
```

## Make gRPC calls

A gRPC call is initiated by calling a method on the client. The gRPC client will handle message serialization and addressing the gRPC call to the correct service.

```csharp
ICalculator calculator = clientFactory.CreateClient<ICalculator>(channel);

// call Sum: sum == 6
var sum = await calculator.Sum(1, 2, 3);

// call MultiplyBy: multiplier == 2, values == [] {2, 4, 6}
var (multiplier, values) = await calculator.MultiplyBy(new[] {1, 2, 3}, 2);
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Examples](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples)
