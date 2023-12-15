# ServiceModel.Grpc.Client.DependencyInjection

`ServiceModel.Grpc` enables applications to communicate with gRPC services using a code-first approach (no [.proto files](https://learn.microsoft.com/en-us/aspnet/core/grpc/basics#proto-file)), helps to get around limitations of gRPC protocol like "only reference types", "exact one input", "no nulls", "no value-types". Provides exception handling. Helps to migrate existing WCF solution to gRPC with minimum effort.

`ServiceModel.Grpc.Client.DependencyInjection` provides client extensions for [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) and [Grpc.Net.ClientFactory](https://www.nuget.org/packages/Grpc.Net.ClientFactory) packages.

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

## Configure Grpc.Net.ClientFactory registrations

After registering a gRPC service client by the generic AddGrpcClient method, configure Grpc.Net.ClientFactory to use ServiceModel.Grpc creator:

```csharp
IServiceCollection services = ...

services
    // Grpc.Net.ClientFactory registration
    .AddGrpcClient<ICalculator>(options =>
    {
        options.Address = new Uri("https://localhost:5001");
    })
    // use ServiceModel.Grpc creator
    .ConfigureServiceModelGrpcClientCreator<ICalculator>((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    });
```

The gRPC service client is registered as transient with dependency injection. The client can now be injected and consumed directly in types created by DI.

```csharp
IServiceProvider serviceProvider = ...
ICalculator calculator = serviceProvider.GetRequiredService<ICalculator>();

// call Sum: sum == 6
var sum = await calculator.Sum(1, 2, 3);

// call MultiplyBy: multiplier == 2, values == [] {2, 4, 6}
var (multiplier, values) = await calculator.MultiplyBy(new[] {1, 2, 3}, 2);
```

## Register and configure gRPC clients with custom GrpcChannel

```csharp
IServiceCollection services = ...;

services.AddSingleton<ChannelBase>(GrpcChannel.ForAddress("http://localhost:5000"));

services
    .AddServiceModelGrpcClient<ICalculator>((options, serviceProvider) =>
    {
        options.MarshallerFactory = ...
        options.ErrorHandler = ...
    });
```

## Links

- [Documentation](https://max-ieremenko.github.io/ServiceModel.Grpc)
- [ServiceModel.Grpc GitHub](https://github.com/max-ieremenko/ServiceModel.Grpc)
- [Examples](https://github.com/max-ieremenko/ServiceModel.Grpc/tree/master/Examples)
